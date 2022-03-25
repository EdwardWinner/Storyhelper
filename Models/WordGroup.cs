using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using StoryHelperLibrary.Helpers;

namespace StoryHelper.Classes
{
    [Serializable()]
    [JsonConverter(typeof(PropertyNameMatchingConverter))]
    public class WordGroup : Commandable, JSerializable<ActionParser>, IComparable, ActionParser, Registrable<ActionParser>, DBActionable, IComparer<WordGroup>
    {
        public int dbID { get; set; }
        public int ownerId { get; set; }
        private Registrar<ActionParser> owner = null;

        internal Registrar<ActionParser> Owner
        {
            get { return owner; }
            set { owner = value; }
        }

        public void setOwner(Registrar<ActionParser> owner)
        {
            if (owner == null) return;
            this.Owner = owner;
        }

        public Registrar<ActionParser> getOwner()
        {
            return this.owner;
        }

        private string groupTitle = "";

        public string GroupTitle
        {
            get { return groupTitle.ToLower(); }
            set { groupTitle = value.ToLower(); }
        }
        private ListDrawString words = new ListDrawString(100);

        public List<string> Words
        {
            get {
                //return this.get();
                this.words.replenish();
                return this.words;
            }
            set {
                //this.add(value);
                words = new ListDrawString(100, value);
            }
        }
        private int groupID = -1;

        public int GroupID
        {
            get { return groupID; }
            set { groupID = value; }
        }
        protected Random r = new Random();

        

        public void add(string adj)
        {
            if (String.IsNullOrEmpty(adj.Trim())) return;
            foreach (string str in this.words)
            {
                if (str.Equals(adj)) {
                    return;
                }
            }
            this.words.Add(adj);
        }

        public void add(string[] adjs)
        {
            if (adjs != null)
                foreach (string str in adjs)
                {

                    this.add(str);
                }
        }

        public void add(List<string> adjs)
        {
            foreach (string str in adjs)
            {
                this.add(str);
            }
        }

        public string[] getAsArray()
        {
            this.words.replenish();
            return this.Words.ToArray<string>();
        }

        public List<string> get()
        {
            this.words.replenish();
            return new List<string>(this.words);
        }

        public void remove(int index)
        {
            if (index > this.words.Count || index < 0) return;
            this.words.RemoveAt(index);
        }

        public void remove(string word)
        {
            if (String.IsNullOrEmpty(word)) return;
            this.words.Remove(word);
        }

        public WordGroup() 
        {
            this.dbID = -1;
            setDelegates();
        }

        public WordGroup(string id): this()
        {
            this.groupTitle = id;
        }

        public WordGroup(string id, List<string> list)
            : this(id)
        {
            foreach (string s in list)
            {
                this.words.Add(s);
            }
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("classType", this.GetType().Name);
            info.AddValue("GroupTitle", this.GroupTitle);
            info.AddValue("Words", this.Words);
            info.AddValue("GroupID", this.GroupID);
            
            
        }

        //Deserialization constructor.
        public WordGroup(SerializationInfo info, StreamingContext ctxt)
        {
            this.dbID = -1;
            //Get the values from info and assign them to the appropriate properties
            try
            {
                this.GroupTitle = (string)info.GetValue("GroupTitle", typeof(string));
            }
            catch (Exception)
            { }
            try
            {
                this.GroupID = (int)info.GetValue("GroupID", typeof(int));
            }
            catch (Exception)
            { }
            try
            {
                this.Words = (List<string>)info.GetValue("Words", typeof(IList<string>));
            }
            catch (Exception)
            { }
        }

        //public virtual int CompareTo(object obj)
        //{
        //    if (obj == null)
        //        return -1;
        //    if (!(obj is WordGroup))
        //        return -1;
        //    return this.getId().CompareTo((obj as WordGroup).getId());

        //}

        public string getId()
        {
            return this.groupTitle.ToLower();
        }

        public string getFullId()
        {
            return this.getId();
        }

        public void setId(string id)
        {
            this.groupTitle = id.ToLower();
        }

        public string getName()
        {
            return this.getId();
        }

        private string parseVerbAssociatedCode(string code, int tense)
        {
            Verb.Gender type = Verb.Gender.female;
            //bool genderChanged = false;
            int pov = -2;

            string up = code.ToUpper();
            if (up.StartsWith("B"))
            {


                if (!int.TryParse(up.Substring(1, 1), out pov))
                {
                    return "";
                }

                int endingNumber = -1; // if the endingNumber of the code is double digit, then it better to strip the code of the 2 first characters rather than figure out if the code has 2 digits or not. 

                up = up.Remove(0, 2);
                if (!int.TryParse(up, out endingNumber))
                {
                    return "";
                }

                // Tense is from -1 to 2. It's easier to parse 0 to 3, but logically the range -1 to 2 is more intuitive linguisitically speaking. 
                endingNumber -= 1;
                var t = Verb.getRelativeTenseFromInt(endingNumber);
                //var r = Verb.getRelativeTense(endingNumber);
                //var d = Verb.getRelativeTenseFromInt(r);

                string verbCommand = Verb.prepareVerbCommand(this.words.draw(), t);

                

                string verby = Verb.getVerb(verbCommand, false, pov, false, Verb.Gender.female, tense);


                string associated = this.words.getLastAssociated(verby);


                return associated;


            }

            return "";
        }

        private string parseVerbCode(string code, int tense)
        {
            int pov = -2;

            string up = code.ToUpper();
            if (up.StartsWith("V"))
            {


                if (!int.TryParse(up.Substring(1, 1), out pov))
                {
                    return "";
                }

                int endingNumber = -1; 

                // if the endingNumber of the code is double digit, then it better to strip the code of the 2 first characters rather than figure out if the code has 2 digits or not. 
                up = up.Remove(0, 2);
                if (!int.TryParse(up, out endingNumber))
                {
                    return "";
                }

                // Tense is from -1 to 2. It's easier to parse 0 to 3, but logically the range -1 to 2 is more intuitive linguisitically speaking. 
                endingNumber -= 1;
                var t = Verb.getRelativeTenseFromInt(endingNumber);
                //var r = Verb.getRelativeTense(endingNumber);
                //var d = Verb.getRelativeTenseFromInt(r);

                string verbCommand = Verb.prepareVerbCommand(this.words.draw(), t);

                string verby = Verb.getVerb(verbCommand, false, pov, false, Verb.Gender.female, tense);

                return verby;


            }

            return "";
        }

        protected override void setDelegates()
        {
            var d = this.getFreshParameterList();
            //d.Add(new MethodPackage.Parameter("Adjective", "The adjective to convert into an adverb.", typeof(string)));
            addDelegate("ADVERB", "Convert an adjective to an adverb.", null, (x, y) => { return Adverb.adjectiveToAdverb(this.drawFromList(this.words, "", 100, true)); });
            d = this.getFreshParameterList();
            //d.Add(new MethodPackage.Parameter("Verb", "The verb to convert into an adverb.", typeof(string)));
            addDelegate("VERBTOADVERB", "Convert a verb to an adverb.", null, (x, y) => { return Adverb.verbToAdverb(this.drawFromList(this.words, "", 100, true)); });
            d = this.getFreshParameterList();
            addDelegate("DL", "draw linearly, sequentially.", null, (x, y) => { return this.drawFromList(this.words, "", 100, true); });
            addDelegate("SL", "Sublist. Gets an associated value if one exists. Otherwise, the key is selected", null, (x, y) => { return evaluateProbability(this.words.getAssociated(ListDrawString.DrawingStyle.Sublist), 100); });
            addDelegate("R", "Legacy. Operates same as SL.", null, (x, y) => { return evaluateProbability(this.words.getAssociated(ListDrawString.DrawingStyle.Sublist), 100); });
            addDelegate("FSL", "ForcedSublist. Forces an associated value. If list contains none, an empty string is returned.", null, (x, y) => { return evaluateProbability(this.words.getAssociated(ListDrawString.DrawingStyle.ForcedSublist), 100); });
            addDelegate("L", "LastSelected. Gets the last adjectinve selected.", null, (x, y) => { return this.words.LastSelected; });
            addDelegate("FB", "ForceBoth, key and associated", null, (x, y) => { return this.words.getAssociated(ListDrawString.DrawingStyle.ForcedBoth); });
            addDelegate("LSL", "LastSublist. Gets the last adjective selected's value in key-value system.", null, (x, y) => { return this.words.getAssociated(ListDrawString.DrawingStyle.LastSublist); });
            addDelegate("LB", "LastBoth. Gets the last adjective selected's value and key.", null, (x, y) => { return this.words.getAssociated(ListDrawString.DrawingStyle.LastBoth); });
            addDelegate("B", "Both. Gets both key and value of a randomly selected item.", null, (x, y) => { return this.words.getAssociated(ListDrawString.DrawingStyle.Both); });

            d = this.getFreshParameterList();
            d.Add(new MethodPackage.Parameter("Key", "The key of the value to be fetched.", typeof(string)));
            addDelegate("KEY", "Gets the Value given a key supplied.", d, (x, y) => {
                string words = this.words.fetch(y[0]);
                return (String.IsNullOrWhiteSpace(words)) ?  y[0] :  words;
            });


            addDelegate("V2A", "Verb to adverb. If more than one word drawn, then returns empty.", null, (x, y) =>
            {
                string adj = this.drawFromList(this.words, "", 100);

                // if the selected word actually contains more than one word, return an empty string. 
                if (adj.Trim().Contains(" ")) return "";
                return Adverb.verbToAdverb(adj);
            });
            addDelegate("A2A", "gets the last adjectinve selected.", null, (x, y) => { return Adverb.adjectiveToAdverb(this.words.draw()); });
            
            addDelegate("A", "gets the last adjective selected.", null, (x, y) => { return getAdverbAdjective(this.words); });
            addDelegate("N", "gets the last adjective selected.", null, (x, y) =>
            {
                string category = this.groupTitle;
                if (category.StartsWith("adj - ") || category.StartsWith("adv - "))
                {
                    category = category.Remove(0, 6);
                }
                if (category.StartsWith("adj- ") || category.StartsWith("adv- "))
                {
                    category = category.Remove(0, 5);
                }
                if (category.StartsWith("adj-") || category.StartsWith("adv-"))
                {
                    category = category.Remove(0, 4);
                }
                if (category.StartsWith("adj - ") || category.StartsWith("adv - "))
                {
                    category = category.Remove(0, 6);
                }
                if (category.StartsWith("noun - ") || category.StartsWith("verb - "))
                {
                    category = category.Remove(0, 7);
                }
                if (category.StartsWith("noun- ") || category.StartsWith("verb- "))
                {
                    category = category.Remove(0, 6);
                }
                if (category.StartsWith("noun -") || category.StartsWith("verb -"))
                {
                    category = category.Remove(0, 6);
                }
                if (category.StartsWith("noun-") || category.StartsWith("verb-"))
                {
                    category = category.Remove(0, 5);
                }
                return category;
            });
            #region Verb Methods
            d = getFreshParameterList();

            addDelegate("V", "Something", null, (x, y) =>
            {
                var relativeTense = Verb.getTenseFromInt(this.tense);
                var verb = new Verb(this.drawFromList(this.words, "", 100, true), false, 3, Classes.Verb.Gender.it, relativeTense);
                return verb.getVerb();
            });

            d = getFreshParameterList();
            d.Add(new MethodPackage.Parameter("Relative tense", "Values accepted are -1, 0, 1, 2", typeof(int)));
            addDelegate("V", "Something", d, (x, y) =>
            {
                var relativeTense = Verb.getTenseFromInt(this.tense + MethodPackage.Parameter.getParameter(y[0]).getValue());
                var verb = new Verb(this.drawFromList(this.words, "", 100, true), false, 3, Classes.Verb.Gender.it, relativeTense);
                return verb.getVerb();
            });

            d = getFreshParameterList();
            d.Add(new MethodPackage.Parameter("Relative tense", "Values accepted are -1, 0, 1, 2", typeof(int)));
            d.Add(new MethodPackage.Parameter("Adverb", "And adverb to slap into the verb phrase.", typeof(string)));
            addDelegate("V", "Something", d, (x, y) =>
            {
                var relativeTense = Verb.getTenseFromInt(this.tense + MethodPackage.Parameter.getParameter(y[0]).getValue());
                var verb = new Verb(this.drawFromList(this.words, "", 100, true), false, 3, Classes.Verb.Gender.it, relativeTense, y[1]);
                return verb.getVerb();
            });
            d = getFreshParameterList();
            d.Add(new MethodPackage.Parameter("Adverb", "And adverb to slap into the verb phrase.", typeof(string)));
            addDelegate("V", "Something", d, (x, y) =>
            {
                var verb = new Verb(this.drawFromList(this.words, "", 100, true), false, 3, Classes.Verb.Gender.it, Verb.getTenseFromInt(this.tense), y[0]);
                return verb.getVerb();
            });
            d = getFreshParameterList();
            d.Add(new MethodPackage.Parameter("Relative tense", "Values accepted are -1, 0, 1, 2", typeof(int)));
            d.Add(new MethodPackage.Parameter("Relative tense", "Values accepted are -1, 0, 1, 2", typeof(int)));
            addDelegate("V", "Verbs", null, (x, y) => { return getAdverbAdjective(this.words); });

            #endregion
        }

        private int tense = 0;
        public string interpret(string code, int tense)
        {
            #region Method Starter
            if (String.IsNullOrWhiteSpace(code)) return evaluateProbability(this.words.getAssociated(ListDrawString.DrawingStyle.Sublist), 100);
            if (code.ToUpper().StartsWith("~")) return code.Remove(0, 1);

            // If the code isn't stripped before entering this method, then the code variable was passed by an already interpreted code which gave this code to be reinterpreted. 
            if (code.StartsWith(this.startCommand) && code.EndsWith(this.endCommand))
            {
                return code;
                //code = code.Remove(0, this.startCommand.Length);
                //code = code.Substring(0, code.Length - this.endCommand.Length);
            }

            // ensure that all methods are properly parsed
            if (code.IndexOf(".") > -1)
            {
                var listOfMethods = parseCommandMethods(code, ".", "(", ")");
                if (listOfMethods.Count > 1) return this.interpret(listOfMethods.ToArray(), tense);
            }

            // set the tense of the object to ensure up-to-date results on verbs
            this.tense = tense;

            // safely parsed methods at this point
            int parenthesesStart, parenthesesEnd;
            string methodName = code.ToUpper();
            List<string> parameters = new List<string>();
            string parameterString = findFirstCommand(code, out parenthesesStart, out parenthesesEnd, "(", ")", false);

            // malformed params or no params skips this
            if (!(parenthesesStart == -1 || parenthesesEnd == -1 || parenthesesEnd < parenthesesStart))
            {

                methodName = code.Substring(0, parenthesesStart).ToUpper().Trim();
                parameters.AddRange(parameterString.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries));
            }

            string result = "";

            // If the wordgroup contains a number as a method name, then apply the default random draw and draw the amount the number indicates
            int methodNumber = -1;
            if (int.TryParse(methodName, out methodNumber))
            {
                if (methodNumber < 3)
                    return this.chainedAdjectives(methodNumber, ", ");
                else
                    return this.chainedAdjectives(methodNumber, ", ", ", and ");
                //evaluateProbability(this.words.getAssociated(ListDrawString.DrawingStyle.Sublist), 100);
            }

            // exact match method names
            try
            {
                if (functions.Keys.Contains(methodName))
                {
                    result = functions[methodName].Invoke(parameters);
                    if (!String.IsNullOrWhiteSpace(result)) return result;
                }
                else
                {
                    return code;
                }
                //result = delegates[methodName].Invoke(methodName, parameters);
            }
            catch (Exception ex)
            {
                // failed to find the method
                return ex.Message;
            }


            return "";
            #endregion
            // ======================================================================Legacy=======================================================================

            #region done
            Regex regexIsAdverb = new Regex(@"^adverb\(\w{1,}\)$", RegexOptions.IgnoreCase);
            if (regexIsAdverb.IsMatch(code))
            {
                string adverb = code.Remove(0, 7);
                adverb = adverb.Remove(adverb.Length - 1, 1);

                return Adverb.adjectiveToAdverb(adverb);
            }

            Regex regexIsVerbToAdverb = new Regex(@"^verbToAdverb\(\w{1,}\)$", RegexOptions.IgnoreCase);
            if (regexIsVerbToAdverb.IsMatch(code))
            {
                string adverb = code.Remove(0, 13);
                adverb = adverb.Remove(adverb.Length - 1, 1);

                return Adverb.verbToAdverb(adverb);
            }

            

            if (code.ToUpper().StartsWith("~"))
                return code.Remove(0, 1);

            int adjectiveCount = 0;

            if (int.TryParse(code, out adjectiveCount))
            {
                if (adjectiveCount < 3)
                    return this.chainedAdjectives(adjectiveCount, ", ");
                else
                    return this.chainedAdjectives(adjectiveCount, ", ", ", and ");
            }

            #endregion

            Regex regexWordgroupVerbCode = new Regex(@"^V\d{1,2}$");
            if (regexWordgroupVerbCode.IsMatch(code))
            {
                return parseVerbCode(code, tense);
            }

            Regex regexWordgroupVerbAssociatedCode = new Regex(@"^B\d{1,2}$");
            if (regexWordgroupVerbAssociatedCode.IsMatch(code))
            {
                return parseVerbAssociatedCode(code, tense);
            }

            Regex regexWordgroupVerbAssociatedCodeWithAdverb = new Regex(@"^B\d{1,2}$");
            if (regexWordgroupVerbAssociatedCode.IsMatch(code))
            {
                return parseVerbAssociatedCode(code, tense);
            }


            switch (code.ToUpper())
            {
                case "DL": // draw linearly, sequentially. 
                    return this.drawFromList(this.words, "", 100, true);// this.chainedAdjectives(1, ""); // + " " + getAliasName();
                case "SL": // gets an associated value if one exists. Otherwise, the key is selected
                    return evaluateProbability(this.words.getAssociated(ListDrawString.DrawingStyle.Sublist), 100);
                case "FSL": // forces an associated value. If list contains none, an empty string is returned.
                    return evaluateProbability(this.words.getAssociated(ListDrawString.DrawingStyle.ForcedSublist), 100);
                case "L": // gets the last adjectinve selected.
                    return this.words.LastSelected;
                case "FB":
                    return this.words.getAssociated(ListDrawString.DrawingStyle.ForcedBoth);
                case "LSL":
                    return this.words.getAssociated(ListDrawString.DrawingStyle.LastSublist);
                case "LB":
                    return this.words.getAssociated(ListDrawString.DrawingStyle.LastBoth);
                case "B":
                    return this.words.getAssociated(ListDrawString.DrawingStyle.Both);
                case "V2A":

                    string adj = this.drawFromList(this.words, "", 100);

                    // if the selected word actually contains more than one word, return an empty string. 
                    if (adj.Trim().Contains(" ")) return "";
                    return Adverb.verbToAdverb(adj);
                
                case "A2A":
                    return Adverb.adjectiveToAdverb(this.words.draw());
                case "A":
                    return getAdverbAdjective(this.words);
                case "N":
                    string category = this.groupTitle;
                    if (category.StartsWith("adj - ") || category.StartsWith("adv - "))
                    {
                        category = category.Remove(0, 6);
                    }
                    if (category.StartsWith("adj- ") || category.StartsWith("adv- "))
                    {
                        category = category.Remove(0, 5);
                    }
                    if (category.StartsWith("adj-") || category.StartsWith("adv-"))
                    {
                        category = category.Remove(0, 4);
                    }
                    if (category.StartsWith("adj - ") || category.StartsWith("adv - "))
                    {
                        category = category.Remove(0, 6);
                    }
                    if (category.StartsWith("noun - ") || category.StartsWith("verb - "))
                    {
                        category = category.Remove(0, 7);
                    }
                    if (category.StartsWith("noun- ") || category.StartsWith("verb- "))
                    {
                        category = category.Remove(0, 6);
                    }
                    if (category.StartsWith("noun -") || category.StartsWith("verb -"))
                    {
                        category = category.Remove(0, 6);
                    }
                    if (category.StartsWith("noun-") || category.StartsWith("verb-"))
                    {
                        category = category.Remove(0, 5);
                    }
                    return category;
                default:
                    return this.words.draw();
            }
        }

        protected string evaluateProbability(string word, int probability)
        {
            int value = r.Next(100);
            if (value > probability) return "";
            return word;
        }

        protected string getVerb(string paramCode, int tense, Verb.Gender gender = Verb.Gender.it, int pointOfView = 3)
        {
            
            //Verb verby = new Verb(paramCode, false, pointOfView, gender, Verb.VerbTense.SimplePast);
            var verby = Verb.prepareVerbCommand(paramCode, Verb.VerbTense.SimplePast);
            string verb = Verb.getVerb(verby.ToString(), false, pointOfView, true, gender, tense);
            if (!String.IsNullOrWhiteSpace(verb)) return verb;
            return "";
        }

        protected string getAdverbAdjective(ListDrawString list)
        {
        //    string theAdjective = this.drawFromList(list, "");
        //    if (String.IsNullOrWhiteSpace(theAdjective)) return "";
            return list.getAssociated(ListDrawString.DrawingStyle.Both);
            //return theAdverb + " " + theAdjective;
        }

        protected string drawFromList(ListDrawString list, string defaultString, int probability = 100, bool linear = false)
        {
            int value = r.Next(100);
            if (value > probability) return defaultString;

            if (list.ListCount() > 0)
            {
                if (!linear) return list.draw();
                else return list.drawLinear();
            }
            else
                return defaultString;
        }

        public string interpret(string[] code, int tense)
        {
            string[] returnValues = new string[code.Length];
            int x = 0;
            foreach (string str in code)
            {
                returnValues[x] = this.interpret(str, tense);
            }

            return String.Join(" ", returnValues);
        }

        public Dictionary<string, string> help()
        {
            Dictionary<string, string> d = new Dictionary<string, string>();
            d.Add("N", "Returns the name of the group.");
            d.Add("1", "Returns a random alias. 2 returns two of them, comma sperated. ");

            return d;
        }

        public string getJson()
        {
            string muhString = Newtonsoft.Json.JsonConvert.SerializeObject(this);
            return muhString;
        }

        public ActionParser getClone()
        {
            //return (ActionParser)Newtonsoft.Json.JsonConvert.DeserializeObject<WordGroup>(this.getJson());

            DBWordGroup dbWordgroup = new DBWordGroup();
            //DBMatter dbMatter = new DBMatter();

            //dbMatter = base.getDBObejct(dbMatter) as DBMatter;

            dbWordgroup = this.getDBObejct(dbWordgroup) as DBWordGroup;
            WordGroup wordGroup = new WordGroup();
            wordGroup.setFromDBObject(dbWordgroup);


            wordGroup.dbID = this.dbID;
            wordGroup.GroupID = this.GroupID;
            wordGroup.GroupTitle = this.GroupTitle;
            wordGroup.Owner = this.Owner;
            wordGroup.ownerId = this.ownerId;
            wordGroup.Words = this.Words;

            return wordGroup;
        }

        private string chainedAdjectives(int amount)
        {
            string str = "";

            if(this.words.ListCount() < amount) amount = this.words.ListCount();
            for (int x = 0; x < amount; x++)
            {
                str += this.words.draw() + ", ";
            }

            if (str.Length > 2)
                str = str.Substring(0, str.Length - 2);

            return str;
        }

        private string chainedAdjectives(int amount, string seperator, string ender = "")
        {
            string[] strs = this.words.draw(amount);
            if (strs.Length == 0) return "";

            if (strs.Length > 2 && ender.Length > 0)
            {
                // pop the last element
                string finalString = strs[strs.Length - 1];
                List<string> strings = new List<string>(strs);
                strings.RemoveAt(strs.Length - 1);
                strs = strings.ToArray();
                return String.Join(seperator, strs) + ender + finalString;
            }

            return String.Join(seperator, strs);
        }

        public override string ToString()
        {
            return this.getId();
        }


        public DBbase getDBObejct(DBbase d)
        {
            DBWordGroup w = new DBWordGroup();
            w.words = String.Join("|", this.words.ToArray());
            w.scriptId = this.getId();
            w.id = this.dbID;
            w.ownerId = this.ownerId;
            

            return w as DBbase;
        }

        public void setFromDBObject(DBbase d)
        {
            DBWordGroup w = d as DBWordGroup;
            this.setId(w.scriptId);
            this.groupTitle = w.scriptId;
            this.dbID = w.id;
            this.ownerId = w.ownerId;
            this.words = new ListDrawString(100, w.words.Split('|').ToList());
        }


        public void setId(long id)
        {
            this.dbID = (int)id;
        }

        public override bool Equals(System.Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            WordGroup p = obj as WordGroup;
            if ((System.Object)p == null)
            {
                return false;
            }

            // check for reference equality.
            if (System.Object.ReferenceEquals(this, p)) return true;

            // Return true if the fields match:

            return (this.getId().Equals(p.getId()));
        }


        public bool FullEquals(System.Object obj)
        {
            if (!this.Equals(obj)) return false;
            return this.words.Count == this.words.Count;
        }

        int IComparable.CompareTo(object obj)
        {
            if (obj == null)
                return -1;
            if (!(obj is WordGroup))
                return -1;
            return this.getId().CompareTo((obj as WordGroup).getId());

        }

        public int Compare(WordGroup x, WordGroup y)
        {

            if (x == null) return -1;
            if (y == null) return 1;
            return x.getId().CompareTo(y.getId());

        }

        public void setActorTense(int tense)
        {
            this.tense = tense;
        }

        public int getActorTense()
        {
            return this.tense;
        }
    }


    



}

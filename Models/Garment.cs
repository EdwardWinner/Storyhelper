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
    [JsonConverter(typeof(PropertyNameMatchingConverter))]
    public class Garment : Matter, StoryHelperLibrary.Interfaces.Mergable<Garment>
    {

        public bool fabricType_readonly { get; set; }
        public bool location_readonly { get; set; }
        public bool description_readonly { get; set; }


        private string fabricType = "";

        public string FabricType
        {
            get { return fabricType; }
            set { 
                fabricType = value;  }
        }

        private string location = "";

        public string Location
        {
            get { return location; }
            set { location = value; }
        }


        private ListDrawString garmentThat_ = new ListDrawString(100, false);

        public List<string> GarmentThat_
        {
            get { return garmentThat_; }
            set { 
                garmentThat_ = new ListDrawString(100, value); 
            }
        }

        public void addGarmentDescription(string desc)
        {
            this.GarmentThat_.Add(desc);
        }

        public void addGarmentDescription(string[] desc)
        {
            foreach (string str in desc)
            {
                this.GarmentThat_.Add(str);
            }
        }

        public string[] getGarmentsArray()
        {
            return getList(this.garmentThat_);
        }

        public Garment() : base() 
		{ 
			this.IsUsed = false;
		
		}

        public Garment(string name)
            : base(name)
        { }

        public Garment(string name, string fabricType)
            : base(name)
        {

            this.FabricType = fabricType;
        }

        public Garment(string name, string fabricType, string location)
            : this(name, fabricType)
        {
            this.Location = location;
        }

        public override ActionParser deepCopy()
        {
            Garment g = new Garment(this.Name);
            this.copy(g as Matter);
            return g as ActionParser;
        }

        protected override Matter copy(Matter m)
        {
            //Console.WriteLine(m.Name +  " = Garment accessed");
            Garment g = m as Garment;
            g.fabricType = this.fabricType;
            g.garmentThat_ = new ListDrawString(100, this.copyList(this.garmentThat_));
            g.location = this.location;
            return base.copy(g);
        }

        public override ActionParser getClone()
        {

            DBGarment dbMatter = new DBGarment();
            this.getDBObejct(dbMatter);
            Garment m = new Garment();
            m.setFromDBObject(dbMatter);
            return m;

            //return this.deepCopy();
            //return (ActionParser)Newtonsoft.Json.JsonConvert.DeserializeObject<Garment>(this.getJson());
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            if (!String.IsNullOrEmpty(this.Location)) info.AddValue("Location", this.Location);
            if (!String.IsNullOrEmpty(this.fabricType)) info.AddValue("FabricType", this.FabricType);
            if (this.GarmentThat_.Count > 0) info.AddValue("GarmentThat_", this.GarmentThat_);
            

            info.AddValue("location_readonly", this.location_readonly);
            info.AddValue("fabricType_readonly", this.fabricType_readonly);
            info.AddValue("description_readonly", this.description_readonly);

        }

        //Deserialization constructor.
        public Garment(SerializationInfo info, StreamingContext ctxt)
            : base(info, ctxt)
        {

            try
            {
                this.Location = (string)info.GetValue("Location", typeof(string));
            }
            catch (Exception)
            { }

            try
            {
                this.FabricType = (string)info.GetValue("FabricType", typeof(string));
            }
            catch (Exception)
            { }
            try
            {
                this.GarmentThat_ = (List<string>)info.GetValue("GarmentThat_", typeof(IList<string>));
            }
            catch (Exception)
            { }
            



            try
            {
                this.location_readonly = (bool)info.GetValue("location_readonly", typeof(bool));
            }
            catch (Exception)
            { }
            try
            {
                this.fabricType_readonly = (bool)info.GetValue("fabricType_readonly", typeof(bool));
            }
            catch (Exception)
            { }
            try
            {
                this.description_readonly = (bool)info.GetValue("description_readonly", typeof(bool));
            }
            catch (Exception)
            { }



        }

        protected override void setDelegates()
        {
            base.setDelegates();
            var d = getFreshParameterList();


            
            addDelegate("MY", "", null, (x, y) =>
            {
                if (this.Owner != null && this.Owner is Matter)
                    return (this.Owner as Matter).interpret("PA", tense);
                return this.interpret("PA", tense);
            });
            addDelegate("FAB", "", null, (x, y) => { return this.FabricType; });
            addDelegate("L", "", null, (x, y) => { return this.Location; });
            addDelegate("GT", "", null, (x, y) => { if (this.garmentThat_.ListCount() > 0) return this.garmentThat_.draw(); return ""; });
            addDelegate("GT-A", "", null, (x, y) =>
            {
                try
                {
                    return this.garmentThat_.draw();
                }
                catch (Exception ex)
                {

                }

                return "";

            });
            d = getFreshParameterList();
            d.Add(new MethodPackage.Parameter("id", "The id of the garment to draw.", typeof(string)));
            addDelegate("GT-A", "", d, (x, y) => { 
                try{
                    return this.garmentThat_.draw(y[0]);
                }catch (Exception ex){

                }

                return "";
                
            });
            addDelegate("GT-F", "", null, (x, y) => { return this.garmentThat_.getAssociated(ListDrawString.DrawingStyle.ForcedSublist); });
            addDelegate("GT-L", "", null, (x, y) => { return this.garmentThat_.LastSelected; });
            addDelegate("FF", "", null, (x, y) => { return "that " + this.drawWho(); });
            addDelegate("FSP", "", null, (x, y) => { return "that " + this.drawWho(); });
            addDelegate("FOP", "", null, (x, y) => { return "that " + this.drawWho(); });
        }

        public override string interpret(string code, int tense)
        {
            return base.interpret(code, tense);
            //if (code.StartsWith("{") && code.EndsWith("}"))
            //{
            //    code = code.Remove(code.Length - 1, 1).Remove(0, 1);
            //}

            Regex regexCallListWithKey = new Regex(@"^GT\-A\(([a-z]|[A-Z]|\d)+\)$", RegexOptions.IgnoreCase);
            if (regexCallListWithKey.IsMatch(code.ToUpper()))
            {
                ListDrawString list = null;
                string theCode = code.ToUpper();
								
				if (code.StartsWith("GT")) list = this.garmentThat_;

				list = this.garmentThat_;
				
				
                string listCode = theCode.Remove(0, 5);
                listCode = listCode.Remove(listCode.Length - 1, 1);

                return list.draw(listCode);
            }


            Verb.Gender gender = Verb.Gender.it;
            string verb = Verb.getVerb(code, this.IsMany, 3, true, gender, tense);
            if (!String.IsNullOrWhiteSpace(verb)) return verb;


            switch (code.ToUpper())
            {
                case "FAB":
                    return this.FabricType;
                case "L":
                    return this.Location;
                case "GT":
                    if (this.garmentThat_.ListCount() > 0)
                        return this.garmentThat_.draw();
                    break;

                case "GT-A":
                    return this.garmentThat_.getAssociated();
                case "GT-F":
                    return this.garmentThat_.getAssociated(ListDrawString.DrawingStyle.ForcedSublist);
                case "GT-L":
                    return this.garmentThat_.LastSelected;

                case "PP":
                    return this.getPossesivePronoun();
                case "PPP":
                    return "the " + this.Name + "'s";
                case "SP":
                    return this.getSubjectPronoun();
                case "SPP":
                    return "the " + this.Name;
                case "OP":
                    return this.getObjectPronouns();
                case "OPP":
                    return "the " + this.Name;
                case "PAP":
                    return "the " + this.Name + "'s";


                
                case "MY":
                    if (this.Owner != null && this.Owner is Matter)
                        return (this.Owner as Matter).interpret("PA", tense);
                    return base.interpret(code, tense);
                
                


                case "THE":
                    return "the";

                case "FF": 
                    return "that " + this.drawWho();

                case "FSP": // stands for fact subject perspective
                    return "that " + this.drawWho();

                case "FOP": // stands for fact Object perspective
                    return "that " + this.drawWho();

                default:
                    return base.interpret(code, tense);
            }

            return base.interpret(code, tense);
        }


        public override Dictionary<string, string> help()
        {
            Dictionary<string, string> dic = base.help();
            dic.Add("F", "Gets the fabric type.");
            dic.Add("L", "Gets the location of the garment");
            dic.Add("GT", "Gets a random garment description.");
            dic.Add("PP", "Gets a personal possesive pronoun.");
            dic.Add("SP", "Gets a subject pronoun.");
            dic.Add("OP", "Gets an object pronoun.");
            dic.Add("PA", "Gets a possessive adjective pronoun.");
            dic.Add("RP", "Gets a reflexive pronoun.");


            return dic;
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public override int CompareTo(object obj)
        {
            return base.CompareTo(obj);
        }


        public override DBbase getDBObejct(DBbase o)
        {
            DBGarment g = o as DBGarment;
            if (g == null) g = new DBGarment();
            g.description = String.Join("|", this.garmentThat_.getExpandedList().ToArray());
            g.location = this.location;
            g.fabricType = this.fabricType;

            g.description_readonly = this.description_readonly ? 1 : 0;
            g.fabricType_readonly = this.fabricType_readonly ? 1 : 0;
            g.isMany_readonly = this.isMany_readonly ? 1 : 0;
            g.location_readonly = this.location_readonly ? 1 : 0;
            g.name_readonly = this.name_readonly ? 1 : 0;
            g.scriptId = this.Id;

            return base.getDBObejct(g);
        }


        public override void setFromDBObject(DBbase o)
        {
            //base.setFromDBObject(o);
            //DBGarment g = o as DBGarment;

            DBGarment g = o as DBGarment;
            if (g == null) g = new DBGarment();
            base.setFromDBObject(g);

            this.location = g.location;
            this.fabricType = g.fabricType;
            ListDrawString.SetStringListFromCSV(garmentThat_, g.description);
            this.setId(g.scriptId);
            this.name_readonly = g.name_readonly == 1 ? true : false;

            this.description_readonly = g.description_readonly == 1 ? true : false;
            this.fabricType_readonly = g.fabricType_readonly == 1 ? true : false;
            this.isMany_readonly = g.isMany_readonly == 1 ? true : false;
            this.location_readonly = g.location_readonly == 1 ? true : false;

        }





        public override bool Equals(System.Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            Garment p = obj as Garment;
            if ((System.Object)p == null)
            {
                return false;
            }

            // check for reference equality.
            if (System.Object.ReferenceEquals(this, p)) return true;

            // Return true if the fields match:

            return this.Id.ToLower().Equals(p.Id.ToLower());
        }


        public void combine(Garment other)
        {
            if (other == null) return;
            foreach (var item in other.Adjectives) this.AddAdjective(item);
            foreach (var item in other.Adverbs) this.addAdverb(item);
            foreach (var item in other.Aliases) this.AddAlias(item);
            foreach (var item in other.States) this.addState(item);
            foreach (var item in other.Who) this.addWho(item);
            foreach (var item in other.GarmentThat_) this.addGarmentDescription(item);

            if (!String.IsNullOrWhiteSpace(other.Location))
            {
                this.Location = other.Location;
            }

            if (!String.IsNullOrWhiteSpace(other.Colour))
            {
                this.Colour = other.Colour;
            }

            if (!String.IsNullOrWhiteSpace(other.FabricType))
            {
                this.FabricType = other.FabricType;
            }
            if (!String.IsNullOrWhiteSpace(other.Size))
            {
                this.Size = other.Size;
            }

            
            
        }

        public void merge(Garment mergedTo, Garment migrant)
        {
            if (mergedTo == null || migrant == null) return;
            base.merge(mergedTo, migrant);

            // do it by reference
            //merge the lists
            mergedTo.addGarmentDescription(mergedTo.GarmentThat_.ToArray());

        }
    }
}

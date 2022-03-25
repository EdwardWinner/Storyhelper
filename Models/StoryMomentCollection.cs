using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using StoryHelperLibrary.Helpers;

namespace StoryHelper.Classes
{
    [JsonConverter(typeof(PropertyNameMatchingConverter))]
    public class StoryMomentCollection : JSerializable<StoryMomentCollection>, DBActionable, IComparable
    {
        public StoryMomentCollection()
        {
            this.cost = -1;
            this.storyname = "";
            this.synopsis = "";
            this.teaser = "";
            this.keywords = "";
            this.dbID = -1;
            this.cover = "";
            
        }

        public StoryMomentCollection(DBStory db):base() 
        {
            this.setFromDBObject(db);
        }

        public int cost { get; set; }
        public string storyname { get; set; }
        public string synopsis { get; set; }
        public string teaser { get; set; }
        public string keywords { get; set; }
        public string cover { get; set; }
        public int dbID { get; set; }
        public int ownerId { get; set; }
        private int tenses { get; set; }
        public int chapter { get; set; }
        public bool tenses_readonly { get; set; } 
        public List<Character> character = new List<Character>();

        public int Tense
        {
            get { return tenses; }
            set { 

                tenses = value; 
            }
        }


        //public int getDBTense()
        //{

        //    return this.Tense;
        //}

        //public void setDBTense(int tense)
        //{
        //    foreach(var sm in this.StoryMoments)
        //    {
        //        sm.Tense = tense;
        //    }

        //    this.Tense = tense;
        //}

        public List<Character> Characters 
        {
            get { return character; }
            set { character = value; }
        }

        private List<StoryMoment> stories = new List<StoryMoment>();

        public List<StoryMoment> StoryMoments
        {
            get 
            {
                
                return stories; 
            }
            set { stories = value; }
        }

        public void add(StoryMoment sm)
        {
            if (!this.stories.Contains(sm))
            {
                this.stories.Add(sm);
            }
            this.stories.Sort();
        }

        public void addCharacter(Character character)
        {
            if (character != null)
            {
                if (!this.Characters.Contains(character))
                {
                    this.Characters.Add(character);
                }
                else
                {
                    int index = this.Characters.IndexOf(character);
                    Character c = this.Characters[index];
                    if (!System.Object.ReferenceEquals(character, c))
                    {
                        c.set(character);
                        //this.Characters.RemoveAt(index);
                        //this.Characters.Insert(index, character);
                    }
                }
            }
        }

        public void addCharacters(List<Character> characters)
        {
            foreach(var c in characters)
            {
                this.addCharacter(c);
            }
        }

        public void removeCharacter(Character character)
        {
            if (character != null && this.Characters.Contains(character))
            {
                this.Characters.Remove(character);
            }
        }

        public void updateCharacters(StoryMoment sm, Human human)
        {
            int momentIndex = this.StoryMoments.IndexOf(sm);

            if(momentIndex > -1)
            {
                if(!System.Object.ReferenceEquals(this.StoryMoments[momentIndex], sm))
                {
                    // Try to update the storymoment without upsetting the references.
                    this.StoryMoments[momentIndex].set(sm);
                }

                Human smHuman = this.StoryMoments[momentIndex].find(human.Character.ScriptId) as Human;

                if (smHuman != null && !System.Object.ReferenceEquals(smHuman.Character, human.Character))
                {
                    smHuman.Character = human.Character;
                }

                if (smHuman != null && this.Characters.Contains(smHuman.Character))
                {
                    Character smcCharacter = this.Characters[this.Characters.IndexOf(smHuman.Character)];
                    if (!System.Object.ReferenceEquals(smcCharacter, smHuman.Character))
                    {
                        // Existing takes precidence. Changes to it should affect all afterwards.
                        smHuman.Character = smcCharacter;

                    }
                }
            }

            
        }

        public string getJson()
        {
            var data = new
            {
                Cost = this.cost,
                Chapter = this.chapter,
                Characters = this.Characters,
                Cover = this.cover,
                Keywords = this.keywords,
                OwnerId = this.ownerId,
                StoryMoments = this.StoryMoments,
                Storyname = this.storyname,
                Synopsis = this.synopsis,
                Teaser = this.teaser,
                Tense = this.Tense,
                ReadonlyTenses = this.tenses_readonly,
                
            };
            //var jSerializerSettings = new JsonSerializerSettings();
            //jSerializerSettings.
            string muhString = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            return muhString;
        }

        public static StoryMomentCollection factory(string json, int userId = -1)
        {
            try
            {
                StoryMomentCollection smc = Newtonsoft.Json.JsonConvert.DeserializeObject<StoryMomentCollection>(json);
                //StoryMomentCollection smc = new StoryMomentCollection();
                //Newtonsoft.Json.JsonConvert.PopulateObject(json, test);
                // What is params? A keyword? What does it do?
                //Newtonsoft.Json.JsonSerializerSettings settings = new Newtonsoft.Json.JsonSerializerSettings();
                //settings.MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Error;
                //dynamic b = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                //string val = (string)b["Pig"];

                //foreach (System.Collections.Generic.IList<Newtonsoft.Json.Linq.JToken> x in b.ChildrenTokens)
                //{
                //    string test = "";
                //    for (var y = 0; y < x.Count; y++)
                //    {
                //        test += x[y].ToString() + ", ";
                //    }
                //    if(!String.IsNullOrWhiteSpace(test)) test = test.Remove(test.Length - 2, 2);

                //    Console.WriteLine(test);
                //}

               // var test = Newtonsoft.Json.JsonConvert.DeserializeObject(json, settings);

                if (smc != null)
                {
                    // Clear imported characters as their references are different from those found...
                    // ... in the Human objects due to the deserialization process. 
                    smc.Characters.Clear();
                    smc.setId(-1);
                    smc.dbID = -1;
                    smc.ownerId = userId;
                    foreach (var story in smc.StoryMoments)
                    {
                        story.Id = -1;
                        smc.tenses = story.Tense;
                        story.storyDescription = smc.synopsis;
                        var smList = story.Items;
                        foreach (var element in smList)
                        {
                            if (element is Human)
                            {
                                (element as Human).dbID = -1;
                                (element as Human).userId = userId;
                                foreach (var trait in (element as Human).Traits)
                                {
                                    trait.dbID = -1;
                                    trait.userId = userId;
                                }

                                foreach (var garment in (element as Human).Garments)
                                {
                                    garment.dbID = -1;
                                    garment.userId = userId;
                                }

                                if ((element as Human).Character != null) 
                                {
                                    (element as Human).Character.Id = -1;
                                    (element as Human).Character.OwnerId = userId;

                                    // Add the character to the main story character list
                                    // Make sure that the story character list is unique
                                    smc.addCharacter((element as Human).Character);
                                }

                                

                            }
                            else if (element is WordGroup)
                            {
                                (element as WordGroup).dbID = -1;
                                (element as WordGroup).ownerId = userId;
                            }
                            else continue;

                            

                        }
                    }
                }

                // Reverse sweep the smc character list and add it to the storyMoments Items.
                foreach (var story in smc.StoryMoments)
                {
                    var smList = story.Humans;
                    foreach (var element in smList)
                    {

                        foreach ( var character in smc.Characters)
                        {
                            if (character.Equals(element.Character))
                            {
                                element.Character = character;
                                break;
                            }
                        }
                    }
                }


                    return smc;
            }
            catch (Exception ex)
            {

                return null;
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("cost", this.cost);
            info.AddValue("cover", this.cover);
            info.AddValue("keywords", this.keywords);
            info.AddValue("storyname", this.storyname);
            info.AddValue("synopsis", this.synopsis);
            info.AddValue("teaser", this.teaser);
            info.AddValue("tenses", this.tenses);
            info.AddValue("tenses_readonly", this.tenses_readonly);
            info.AddValue("chapter", this.chapter);
            //info.AddValue("dbID", this.dbID);
            if (this.stories.Count > 0) info.AddValue("StoryMoments", this.StoryMoments);
        }

        public StoryMomentCollection(SerializationInfo info, StreamingContext ctxt)
        {

            try
            {
                this.cost = (int)info.GetValue("cost", typeof(int));
            }
            catch (Exception)
            { }
            try
            {
                this.chapter = (int)info.GetValue("chapter", typeof(int));
            }
            catch (Exception)
            { }
            try
            {
                this.storyname = (string)info.GetValue("storyname", typeof(string));
            }
            catch (Exception)
            { }
            try
            {
                this.synopsis = (string)info.GetValue("synopsis", typeof(string));
            }
            catch (Exception)
            { }
            try
            {
                this.teaser = (string)info.GetValue("teaser", typeof(string));
            }
            catch (Exception)
            { }
            try
            {
                this.keywords = (string)info.GetValue("keywords", typeof(string));
            }
            catch (Exception)
            { }
            try
            {
                this.cover = (string)info.GetValue("cover", typeof(string));
            }
            catch (Exception)
            { }
            //try
            //{
            //    this.dbID = (int)info.GetValue("dbID", typeof(int));
            //}
            //catch (Exception)
            //{ }

            try
            {
                this.tenses = (int)info.GetValue("tenses", typeof(int));
            }
            catch (Exception)
            { }

            try
            {
                this.tenses_readonly = (bool)info.GetValue("tenses_readonly", typeof(bool));
            }
            catch (Exception)
            { }


            try
            {
                List<StoryMoment> list = new List<StoryMoment>();
                list = (List<StoryMoment>)info.GetValue("StoryMoments", typeof(IList<StoryMoment>));
                foreach (StoryMoment t in list)
                {
                    t.Storyname = this.storyname;
                    this.stories.Add(t);

                }
            }
            catch (Exception)
            { }

        }

        public DBbase getDBObejct(DBbase d)
        {
            DBStory c = d as DBStory;
            if (c == null) c = new DBStory();


            c.storyname = this.storyname;
            c.synopsis = this.synopsis;
            c.teaser = this.teaser;
            c.ownerId = this.ownerId;
            c.keywords = this.keywords;
            c.id = this.dbID;
            c.cover = this.cover;
            c.chapter = this.chapter;
            c.cost = this.cost;
            c.default_tenseId = this.Tense;
            c.tenses_readonly = this.tenses_readonly ? 1 : 0;

            return c;
        }

        public void setFromDBObject(DBbase d)
        {
            var c = d as DBStory;
            if (c == null) return;
            this.storyname = c.storyname;
            this.synopsis = c.synopsis;
            this.teaser = c.teaser;
            this.ownerId = c.ownerId;
            this.keywords = c.keywords;
            this.dbID = c.id;
            this.cover = c.cover;
            this.cost = c.cost;
            this.chapter = c.chapter;
            this.Tense = (c.default_tenseId);
            this.tenses_readonly = c.tenses_readonly == 1 ? true : false;
        }


        public StoryMomentCollection getClone()
        {
            DBStory dbMatter = new DBStory();
            this.getDBObejct(dbMatter);
            StoryMomentCollection h = new StoryMomentCollection();
            h.setFromDBObject(dbMatter);

            foreach (var ch in this.Characters)
            {
                h.Characters.Add(ch.getClone());
            }

            //foreach (var ch in this.UserCharacters)
            //{
            //    h.UserCharacters.Add(ch.getClone());
            //}

            return h;
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
                return -1;
            if (!(obj is StoryMomentCollection))
                return -1;
            if (obj.Equals(this))
                return 0;

            //string formatter = "{0}, {1} {2}";
            //string thisObject = String.Format(formatter, this.FamilyName, this.FirstName, this.MiddleName);
            //string otherObject = String.Format(formatter, ((Human)obj).FamilyName, ((Human)obj).FirstName, ((Human)obj).MiddleName);

            string formatter = "{0}, {1}";
            string thisObject = String.Format(formatter, this.storyname, this.ownerId.ToString());
            string otherObject = String.Format(formatter, ((StoryMomentCollection)obj).storyname, ((StoryMomentCollection)obj).ownerId.ToString());


            return thisObject.CompareTo(otherObject); ;
        }

        public override bool Equals(System.Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            StoryMomentCollection p = obj as StoryMomentCollection;
            if ((System.Object)p == null)
            {
                return false;
            }

            // check for reference equality.
            if (System.Object.ReferenceEquals(this, p)) return true;

            // Return true if the fields match:

            return
                (this.storyname.Equals(p.storyname)) &&
                (this.ownerId == p.ownerId);
        }


        public string getId()
        {
            throw new NotImplementedException();
        }

        public void setId(long id)
        {
            this.dbID = (int)id;
        }
    }
}

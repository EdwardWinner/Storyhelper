using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using static StoryHelper.Classes.ThreaderBase;
using Newtonsoft.Json;
using StoryHelperLibrary.Helpers;

namespace StoryHelper.Classes
{
    [JsonConverter(typeof(PropertyNameMatchingConverter))]
    public class StoryMoment : EventBase, IComparable, DBActionable, JSerializable<StoryMoment>
    {
        private string story = "";
        private List<Registrable<ActionParser>> registrables = new List<Registrable<ActionParser>>();
        
        public int storiesId { get; set; }
        public string picture { get; set; }
        public int globalPercentage { get; set; }
        public int charactersId { get; set; }
        public string characterScriptId = "";
        public string storyDescription = "";
        public Settings settings = new Settings();
        public StringBuilder errLog = new StringBuilder();
        public List<WordGroup> WordGroups { get; set; } = new List<WordGroup>();
        public List<Human> Humans { get; set; } = new List<Human>();


        public StoryMoment()  
        {
            this.storiesId = -1;
            this.picture = "";
            this.globalPercentage = 100;
        }

        public StoryMoment(DBMoment db, Settings settings): this()
        {
            this.storiesId = -1;
            this.setFromDBObject(db);
            this.settings = settings;
            this.picture = "";
            this.globalPercentage = 100;
        }

        public StoryMoment(string storyName, int storyMomentId, Settings settings): this()
        {
            this.storiesId = -1;
            this.storyMomentId = storyMomentId;
            this.storyname = storyName;
            this.settings = settings;
            this.picture = "";
            this.globalPercentage = 100;
        }

        public string Story
        {
            get { return story; }
            set { story = value; }
        }

        private int tense = 2;

        public int Tense
        {
            get { return tense; }
            set {
                this.setTense(value);
            }
        }

        public int getTense()
        {
            return this.tense;
        }

        public void setTense(int tense)
        {
            if (tense == Verb.getTenseDictionary()[Verb.VerbTense.Future] || tense == Verb.getTenseDictionary()[Verb.VerbTense.Present] || tense == Verb.getTenseDictionary()[Verb.VerbTense.SimplePast])
            {
                this.tense = tense;
            }
            return;
        }


        private List<Registrable<ActionParser>> items = new List<Registrable<ActionParser>>();
        public List<Registrable<ActionParser>> Items 
        { 
            get 
            {
                items.Clear();
                items.AddRange(this.Humans);
                items.AddRange(this.WordGroups);
                items.AddRange(this.registrables);
                return items;

            }
            set
            {
                this.Humans.Clear();
                this.WordGroups.Clear();
                this.registrables.Clear();
                foreach( var i in value)
                {
                    this.addActor(i);
                }
            }
        } 
        


        private string storyname = "";

        public string Storyname
        {
            get { return storyname; }
            set { storyname = value; }
        }
        private int storyMomentId = -1;

        public int MomentId
        {
            get { return storyMomentId; }
            set { storyMomentId = value; }
        }
        private int id = -1;

        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        private string momentDescription = "";

        public string Description
        {
            get { return momentDescription; }
            set { momentDescription = value; }
        }

        //public void saveActors()
        //{
        //    MySqlController my = new MySqlController(settings);
        //    //this.setLinkedPOVs();
        //    my.storeMyActors(this);
        //}

        public void loadActors()
        {
            if (String.IsNullOrWhiteSpace(settings.databaseName)) return;
            MySqlController my = new MySqlController(settings);
            Dictionary<string, Registrable<ActionParser>> dic = my.getMyStoryActors(this);

            //if (parentStory != null)
            //{
            //    parentStory.UserCharacters = dic.Values
            //}

            // set the pov of the characters
            //this.setLinkedPOVgivenDictionary(dic);
        }

        public void set(StoryMoment other)
        {

            this.characterScriptId = other.characterScriptId;
            this.charactersId = other.charactersId;
            this.Description = other.Description;
            this.globalPercentage = other.globalPercentage;
            this.Humans = other.Humans;
            this.id = other.id;
            this.Id = other.Id;
            this.Items = other.Items;
            this.MomentId = other.MomentId;
            this.picture = other.picture;
            this.registrables = other.registrables;
            this.settings = other.settings;
            this.storiesId = other.storiesId;
            this.story = other.story;
            this.Story = other.Story;
            this.storyDescription = other.storyDescription;
            this.storyMomentId = other.storyMomentId;
            this.storyname = other.storyname;
            this.Storyname = other.Storyname;
            this.Tense = other.Tense;
            this.tense = other.tense;
            this.WordGroups = other.WordGroups;

            this.Update();
        }


        public void Update()
        {

            var smList = this.Humans;
            foreach (var human1 in smList) 
            {
                
                if (!String.IsNullOrWhiteSpace(human1.Character.LinkedCharacterName))
                {
                    foreach (var human2 in smList)
                    {
                       
                        if (human1.Equals(human2)) continue;

                        try
                        {

                            if (human2.Character.ScriptId == human1.Character.LinkedCharacterName)
                            {
                                // check if the characters are pointing eachother
                                if (human2.Character.LinkedCharacterName == human1.Character.ScriptId)
                                {
                                // if characters point eachother, then remove the linked character from the other
                                human2.Character.LinkedCharacterName = "";
                                }



                                if (!human2.perspective_readonly && !human1.perspective_readonly)
                                {
                                    human1.Perspective = human2.Perspective;
                                    human1.Character.Perspective = human2.Character.Perspective;
                                }
                            }

                        }
                        catch (Exception)
                        {

                        }
                    }
                }
            }


            doSmartReplacers();
            }



        private Dictionary<string, Registrable<ActionParser>> getActorDictionary()
        {
            Dictionary<string, Registrable<ActionParser>> dic = new Dictionary<string,Registrable<ActionParser>>();
            var smList = this.Humans;
            foreach(var item in smList)
            {
                if(item is Human)
                {
                    try
                    {
                        dic.Add((item as Human).Character.ScriptId, item);
                    }
                    catch (Exception) { }

                }
            }

            return dic;
        }

        public void doSmartReplacers()
        {
            var smList = this.Items;
            

            foreach (ActionParser actor in smList)
            {
                if (actor is Registrar<ActionParser>)
                {
                    Registrable<ActionParser> other = null;
                    if (actor is Matter && actor is ActionParser)
                    {
                        other = null;

                        if (actor is Human)
                        {
                            foreach (Registrable<ActionParser> x in smList)
                            {
                                if (x.getId().Equals((actor as Human).Other))
                                {
                                    other = x;
                                    break;
                                }
                            }

                            // Set other
                            if (other != null)
                            {
                                (actor as Human).setOther(other);
                            }



                        }

                        if (!String.IsNullOrWhiteSpace((actor as Matter).Other))
                        {
                            replaceStringWithinActor((actor as ActionParser), (actor as Matter).Other, "other");
                        }
                        else
                        {
                            replaceStringWithinActor((actor as ActionParser), (actor as Matter).getFullId(), "other");
                        }
                        replaceStringWithinActor((actor as ActionParser), actor.getId(), "this");
                        if ((actor as Human).isPov) replaceStringWithinActor((actor as ActionParser), (actor as Human).getFullId(), "main");

                    }

                    foreach (KeyValuePair<string, ActionParser> kp in (actor as Registrar<ActionParser>).Registry)
                    {

                        string ownerId = getOwnerId(kp.Value);
                        string otherId = getOtherId(kp.Value);
                        string thisId = kp.Key;

                        if (!String.IsNullOrEmpty(thisId) && !String.IsNullOrEmpty(ownerId) && !(thisId.Equals(ownerId)))
                        {
                            replaceStringWithinActor(kp.Value, ownerId + "." + thisId, "this");
                        }
                        if (!String.IsNullOrEmpty(ownerId))
                        {
                            replaceStringWithinActor(kp.Value, otherId,  "other");
                        }
                        if (!String.IsNullOrEmpty(otherId))
                        {
                            replaceStringWithinActor(kp.Value, ownerId, "owner");
                        }
                        

                        //if (mainCharacter != null) replaceStringWithinActor(kp.Value, mainCharacter.getFullId(), "main");
                    }
                }
            }
        }

        private string getOwnerId(ActionParser actor)
        {
            if (actor == null) return "";
            if (actor is Matter)
            {
                var m = (actor as Matter).getOwner();
                if (m == null) return (actor as Matter).getOwnerId();
                else
                {
                    return getOwnerId((m as Matter));
                }

            }

            return "";
        }

        private string getOtherId(ActionParser actor)
        {
            if (actor is Matter)
            {
                var m = (actor as Matter).getOwner();
                if (String.IsNullOrEmpty((actor as Matter).Other))
                {
                    if (m is Matter)
                    {
                        Matter mm = (m as Matter);
                        if (!String.IsNullOrEmpty(mm.Other))
                        {
                            return mm.Other;
                        }
                    }
                    if (!String.IsNullOrEmpty(m.getOwnerId()))
                    {
                        return m.getOwnerId();
                    }
                }
                else
                    return (actor as Matter).Other;
            }

            return "";
        }

        private void replaceStringWithinActor(ActionParser actor, string lookFor, string changeTo)
        {

            if (actor is Human)
            {
                Human m = (actor as Human);
                if (m.Colour != null) m.Colour = m.Colour.Replace(lookFor, changeTo);
                this.doChangeOwner(m.Adjectives, changeTo, lookFor);
                this.doChangeOwner(m.Aliases, changeTo, lookFor);
                this.doChangeOwner(m.States, changeTo, lookFor);
                this.doChangeOwner(m.Adverbs, changeTo, lookFor);
                this.doChangeOwner(m.Who, changeTo, lookFor);
                this.doChangeOwner(m.Scripts, changeTo, lookFor);
            }

            if (actor is Trait)
            {
                Trait t = (actor as Trait);
                this.doChangeOwner(t.Adjectives, changeTo, lookFor);
                this.doChangeOwner(t.Aliases, changeTo, lookFor);
                this.doChangeOwner(t.States, changeTo, lookFor);
                this.doChangeOwner(t.Adverbs, changeTo, lookFor);
                this.doChangeOwner(t.Who, changeTo, lookFor);

            }
            if (actor is Garment)
            {
                Garment t = (actor as Garment);
                this.doChangeOwner(t.Adjectives, changeTo, lookFor);
                this.doChangeOwner(t.Aliases, changeTo, lookFor);
                this.doChangeOwner(t.GarmentThat_, changeTo, lookFor);
                this.doChangeOwner(t.States, changeTo, lookFor);
                this.doChangeOwner(t.Adverbs, changeTo, lookFor);
                this.doChangeOwner(t.Who, changeTo, lookFor);
                t.FabricType = t.FabricType.Replace(changeTo, lookFor);
                t.Location = t.Location.Replace(lookFor, changeTo);
            }
            if (actor is WordGroup)
            {
                WordGroup w = (actor as WordGroup);
                this.doChangeOwner(w.Words, changeTo, lookFor);
            }
        }

        private void doChangeOwner(IList<string> list, string ownerId, string lookFor)
        {
            if (list is ListDrawString)
            {
                (list as ListDrawString).massReplace(ownerId, lookFor, this.settings.startCommand, this.settings.endCommand);
            }
            else
            {

                for (int x = list.Count - 1; x >= 0; x--)
                {
                    if (list[x].ToLower().IndexOf(lookFor.ToLower()) > -1 && list[x].Contains(this.settings.startCommand) && list[x].Contains(this.settings.endCommand))
                    {
                        string str = list[x].Replace(lookFor, ownerId);
                        list.RemoveAt(x);
                        list.Add(str);

                    }
                }
            }

        }

        //private void setLinkedPOVgivenDictionary(Dictionary<string, Registrable<ActionParser>> dic)
        //{
        //    // set the pov of the characters
        //    foreach (var entry in dic)
        //    {
        //        if ((entry.Value is Human))
        //        {
        //            Human linked = this.setLinkedPOV(dic, (entry.Value as Human).Character);
        //            if (linked != null)
        //            {
        //                (entry.Value as Human).Perspective = linked.Perspective;
        //            }
        //        }
        //    }
        //}

        //private Human setLinkedPOV(Dictionary<string, Registrable<ActionParser>> dic, Character other)
        //{
        //    foreach (var entry in dic)
        //    {
        //        if ((entry.Value is Human))
        //        {
        //            if (other.linkedCharacterId > 0)
        //            {
        //                if ((entry.Value as Human).Character.id == other.linkedCharacterId && !(entry.Value as Human).Character.Equals(other))
        //                {
        //                    return (entry.Value as Human);
        //                }
        //            }
        //            else if (!String.IsNullOrWhiteSpace(other.linkedCharacterName))
        //            {
        //                if ((entry.Value as Human).Character.scriptId.Equals(other.linkedCharacterName) && !(entry.Value as Human).Character.Equals(other))
        //                {
        //                    return (entry.Value as Human);
        //                }
        //            }
        //        }
        //    }

        //    return null;
        //}

        public void save(int storyId)
        {
            //setLinkedPOVs();
            MySqlController my = new MySqlController(this.settings);
            if (!this.settings.hasConnected)
            {
                throw new Exception("Failed to connect");
            }

            this.storiesId = storyId;
            int storyMomentId = (int)my.storeStoryMoment(this);

            // storyId could be 1 because the return value is either the amount of rows affected, or the new id.
            if (storyMomentId > 1)
            {
                this.id = storyMomentId;
                my.storeMyActors(this);
            }
            else if (storyMomentId == 1)
            {
                my.storeMyActors(this);
            }
            else
            {
                throw new Exception("Failed to get Moment Id.");
            }
        }

        /// <summary>
        /// Finds a Matter object
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Matter or null if not found</returns>
        public ActionParser find(string id)
        {
            var result = this.Items.Where(y => id.ToLower().StartsWith(y.getFullId().ToLower()));
            foreach (Registrable<ActionParser> m in result)
            {
                if (String.Equals(m.getFullId(), id.ToLower(), StringComparison.OrdinalIgnoreCase))
                {
                    if(m is ActionParser)
                    {
                        return m as ActionParser;
                    }
                }
                if(m is Matter)
                {
                    var item = (m as Matter).find(id);
                    if (item != null)
                    {
                        return item;
                    }
                }
                    
            }

            return null;
        }

        public void addActor(Registrable<ActionParser> actor)
        {
            if (actor == null) return;
            if (actor is Human && !this.Humans.Contains(actor as Human))
            {
                this.Humans.Add(actor as Human);
                return;
            }
            if (actor is WordGroup && !this.WordGroups.Contains(actor as WordGroup))
            {
                this.WordGroups.Add(actor as WordGroup);
                return;
            }
            if (!this.registrables.Contains(actor)) 
            {
                this.registrables.Add(actor);
            }
        }
 
        public void removeActor(Registrable<ActionParser> actor)
        {
            if (actor != null)
            {
                if (actor is Human)
                {
                    if (this.Humans.Contains(actor as Human)) this.Humans.Remove(actor as Human);
                }
                else if (actor is WordGroup)
                {
                    if (this.WordGroups.Contains(actor as WordGroup)) this.WordGroups.Remove(actor as WordGroup);
                }
            }
        }

        public override string ToString()
        {
            return this.Storyname + " - " + this.MomentId.ToString();
        }


        public int CompareTo(object obj)
        {
            if (!(obj is StoryMoment)) return 1;

            if ((obj as StoryMoment).Storyname.CompareTo(this.Storyname) == 0)
            {
                if ((obj as StoryMoment).storyMomentId < this.storyMomentId) return 1;
                if ((obj as StoryMoment).storyMomentId == this.storyMomentId) return 0;
                return -1;
            }
            else
            {
                return this.Storyname.CompareTo((obj as StoryMoment).Storyname);
            }
        }

        public string getJson()
        {
            string muhString = Newtonsoft.Json.JsonConvert.SerializeObject(this);
            return muhString;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (this.Id != -1) info.AddValue("Id", this.Id);

            info.AddValue("MomentId", this.MomentId);
            if (!String.IsNullOrEmpty(this.Story)) info.AddValue("PieceOfStory", this.Story);
            if (!String.IsNullOrEmpty(this.Description)) info.AddValue("Description", this.Description);
            if (!String.IsNullOrEmpty(this.Storyname)) info.AddValue("Storyname", this.Storyname);
            if (this.globalPercentage > 0) info.AddValue("globalPercentage", this.globalPercentage);
            if (this.charactersId > 0) info.AddValue("charactersId", this.charactersId);
            info.AddValue("characterScriptId", this.characterScriptId);
            info.AddValue("tense", this.tense);
            
            info.AddValue("picture", this.picture);
            info.AddValue("storiesId", this.storiesId);

            List<Human> humans = new List<Human>();
            List<WordGroup> wordgroups = new List<WordGroup>();

            foreach (object o in this.Items)
            {
                if (o is Human)
                {
                    humans.Add(o as Human);
                }

                else if (o is WordGroup)
                {
                    wordgroups.Add(o as WordGroup);
                }
            }

            if (humans.Count > 0) info.AddValue("Humans", humans);
            if (wordgroups.Count > 0) info.AddValue("Wordgroups", wordgroups);



            
        }

        //Deserialization constructor.
        public StoryMoment(SerializationInfo info, StreamingContext ctxt) : this()
        {
            this.storiesId = -1;
            //Get the values from info and assign them to the appropriate properties
            try
            {
                this.id = (int)info.GetValue("Id", typeof(int));
            }
            catch (Exception)
            {}
            try
            {
                this.charactersId = (int)info.GetValue("charactersId", typeof(int));
            }
            catch (Exception)
            { }
            try
            {
                this.characterScriptId = (string)info.GetValue("characterScriptId", typeof(string));
            }
            catch (Exception)
            { }
            try
            {
                this.Story = (string)info.GetValue("PieceOfStory", typeof(string));
            }
            catch (Exception)
            { }
            try
            {
                this.Description = (string)info.GetValue("Description", typeof(string));
            }
            catch (Exception)
            { }
            try
            {
                this.Storyname = (string)info.GetValue("Storyname", typeof(string));
            }
            catch (Exception)
            { }
            try
            {
                this.MomentId = (int)info.GetValue("MomentId", typeof(int));
            }
            catch (Exception)
            { }
            try
            {
                this.storiesId = (int)info.GetValue("storiesId", typeof(int));
            }
            catch (Exception)
            { }
            try
            {
                this.picture = (string)info.GetValue("picture", typeof(string));
            }
            catch (Exception)
            { }
            try
            {
                this.globalPercentage = (int)info.GetValue("globalPercentage", typeof(int));
            }
            catch (Exception)
            { }

            try
            {
                List<Human> list = new List<Human>();
                list = (List<Human>)info.GetValue("Humans", typeof(List<Human>));
                foreach (Human t in list)
                {
                    this.Items.Add(t);
                }
            }
            catch (Exception)
            { }

            try
            {
                List<WordGroup> list = new List<WordGroup>();
                list = (List<WordGroup>)info.GetValue("Wordgroups", typeof(List<WordGroup>));
                foreach (WordGroup t in list)
                {
                    this.Items.Add(t);
                }
            }
            catch (Exception)
            { }

            try {
                this.tense = (int)info.GetValue("tense", typeof(int));
            }
            catch (Exception) { }

        }

        public override bool Equals(System.Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            StoryMoment p = obj as StoryMoment;
            if ((System.Object)p == null)
            {
                return false;
            }

            // check for reference equality.
            if (System.Object.ReferenceEquals(this, p)) return true;

            // Return true if the fields match:

            return
                (this.MomentId.Equals(p.MomentId)) &&
                (this.storyname.Equals(p.storyname));
        }

        public DBbase getDBObejct(DBbase d)
        {
            DBMoment c = new DBMoment();

            c.id = this.id;
            c.momentId = this.MomentId;
            c.picture = this.picture;
            c.storiesId = this.storiesId;
            c.story = this.Story;
            //c.tense = getDBTense();
            c.description = this.Description;
            c.globalPercentage = this.globalPercentage;
            c.charactersId = this.charactersId;

            return c;
        }

        public void setFromDBObject(DBbase d)
        {
            DBMoment c = d as DBMoment;
            if(c == null) return;
            this.Description = c.description;
            this.globalPercentage = c.globalPercentage;
            this.Id = c.id;
            this.MomentId = c.momentId;
            this.picture = c.picture;
            this.storiesId = c.storiesId;
            this.Story = c.story;
            this.charactersId = c.charactersId;
            //this.Tense = 
            //setDBTense(c.tense);
        }


        public long getId()
        {
            return this.id;
        }

        public void setId(long id)
        {
            this.Id = (int)id;
        }


        public StoryMoment getClone()
        {
            DBMoment dbMatter = new DBMoment();
            dbMatter = this.getDBObejct(dbMatter) as DBMoment;
            StoryMoment storyMoment = new StoryMoment();
            storyMoment.setFromDBObject(dbMatter);


            var smList = this.Items;
            foreach (var item in smList)
            {
                var clonedItem = (item as JSerializable<ActionParser>).getClone();
                

                if (clonedItem is Matter)
                {
                    (clonedItem as Matter).selfRegisterAll();
                }
                storyMoment.addActor(clonedItem as Registrable<ActionParser>);
            }

            

            storyMoment.tense = this.tense;
            storyMoment.storyname = this.storyname;
            storyMoment.storiesId = this.storiesId;
            storyMoment.settings = this.settings;



            return storyMoment;
        }

        /// <summary>
        ///     Checks if one storymoment has any reference equality to the current one being used in variable this.storymoment
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool passCloneTest(StoryMoment s, StoryMoment otherStory)
        {
            if (System.Object.ReferenceEquals(s, otherStory)) return false;
            Dictionary<string, ActionParser> registeredObjectsForHuman = null;
            Dictionary<string, ActionParser> registeredObjectsForOther = null;
            foreach (var item in s.Items)
            {
                if (item is Human)
                {
                    registeredObjectsForHuman = (item as Human).getDictionaryOfRegisteredObjects();
                    foreach (var other in otherStory.Items)
                    {
                        if (other is Human)
                        {
                            var human = (item as Human);
                            var humanOther = (other as Human);
                            
                            registeredObjectsForOther = humanOther.getDictionaryOfRegisteredObjects();
                            foreach (var item2 in registeredObjectsForHuman)
                            {
                                foreach (var other2 in registeredObjectsForOther)
                                {
                                    if (System.Object.ReferenceEquals(other2, item2))
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return true;
        }


        
    }
}

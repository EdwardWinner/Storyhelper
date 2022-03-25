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
    public class Human : Being, StoryHelperLibrary.Interfaces.Mergable<Human>
    {
        public Human MainChar = null;
        private int _charactersId = -1;
        private int perspective = 3;
        public Human() : base()
        {
            setClassConstants();
        }

        public Human(string firstName, string familyName) : this()
        {
            this.FirstName = firstName;
            this.FamilyName = familyName;
            this.Age = 18;
        }

        public Human(string firstName, string middleName, string familyName) : this(firstName, familyName) {
            this.MiddleName = middleName;
            setClassConstants();
        }

        //Deserialization constructor.
        public Human(SerializationInfo info, StreamingContext ctxt)
            : base(info, ctxt)
        {

            try
            {
                this.Character = (Character)info.GetValue("Character", typeof(Character));
            }
            catch (Exception)
            { }

            //try
            //{
            //    this.UserModifiedCharacter = (UserCharacter)info.GetValue("UserCharacter", typeof(UserCharacter));
            //}
            //catch (Exception)
            //{ }
            //try
            //{
            //    List<Trait> list = new List<Trait>();
            //    list = (List<Trait>)info.GetValue("Traits", typeof(IList<Trait>));
            //    foreach (Trait t in list)
            //    {
            //        this.addTrait(t);
            //    }
            //}
            //catch (Exception)
            //{ }
            try
            {
                //this.Garments = (List<IRegistry<ActionParser>>)info.GetValue("Garments", typeof(IList<Garment>));

                List<Garment> list = new List<Garment>();
                list = (List<Garment>)info.GetValue("Garments", typeof(IList<Garment>));
                foreach (Garment t in list)
                {
                    this.addGarment(t);
                }
            }
            catch (Exception)
            { }
            try
            {
                List<string> x = (List<string>)info.GetValue("Scripts", typeof(IList<string>));
                foreach (string str in x)
                {
                    this.addScripts(str);
                }

            }
            catch (Exception) { }
        }

        public const string defaultFirstName = "New Name";
        public const string defaultLastName = "New Name";
        public const string defaultMiddleName = "New Name";
        public const string defaultSpecies = "human being";
        public const string defaultScriptId = "new";


        public bool hasVagina_readonly { get; set; } = false;
        public bool firstName_readonly { get; set; } = false;
        public bool middleName_readonly { get; set; } = false;
        public bool familyName_readonly { get; set; } = false;
        public bool perspective_readonly { get; set; } = false;
        public bool isAnIt_readonly { get; set; } = false;
        public bool scripts_readonly { get; set; } = false;
        public bool HasVagina { get; set; } = false;

        public int charactersId {
            get
            {
                if (this.Character != null) return this.Character.Id;
                return _charactersId;
            }
            set {
                this._charactersId = value;
            }
        }
        public bool isPov { get; set; } = false;
        
        //public bool MainCharacter
        //{
        //    // Idea: change this to boolean and give every character a reference to the main list of characters. 
        //    // Make sure that the list is never changed so that the characters do not lose the reference to the original list.
        //    //get
        //    //{
        //    //    return mainCharacter;
        //    //}
        //    //set
        //    //{
        //    //    this.mainCharacter = value;
        //    //    if (value != null && value.Equals(this)) isPov = true;

        //    //}

        //    get; set;
        //}

        private Character character = null;

        public Character Character
        {
            get { return character; }
            set
            {
                if (value != null)
                {
                    if(this.character == null)
                    {
                        this.setCharacterByReference(value);
                    }
                    Character old = this.character;
                    //this.character = value;
                    this.character.set(value);

                    this.FamilyName = this.character.FamilyName;
                    this.FirstName = this.character.FirstName;
                    this.MiddleName = this.character.MiddleName;

                    if (!String.IsNullOrWhiteSpace(this.character.Colour)) this.Colour = this.character.Colour;
                    if (!String.IsNullOrWhiteSpace(this.character.Height)) this.Height = this.character.Height;
                    if (!String.IsNullOrWhiteSpace(this.character.Size)) this.Size = this.character.Size;
                    if (this.character.Weight != 0) this.Weight = this.character.Weight;
                    if (!String.IsNullOrWhiteSpace(this.character.Colour)) this.Colour = this.character.Colour;
                    if (!String.IsNullOrWhiteSpace(this.character.Height)) this.Height = this.character.Height;
                    this.IsAnIt = this.character.IsAnIt;
                    this.IsMany = this.character.IsMany;
                    this.HasVagina = this.character.HasVagina;
                    this.pronouns = new Pronouns(Pronouns.getPronounEnumFromString(this.character.Pronouns));
                    this.charactersId = this.character.Id;
                    this.Id = this.character.ScriptId;
                    this.Perspective = this.character.Perspective;
                    this.setAgeAndSpecies(this.character.Age);

                    this.FamilyName = this.character.FamilyName;
                    this.FirstName = this.character.FirstName;
                    this.MiddleName = this.character.MiddleName;

                    // Replace the defaults with an empty string, but only after setting the strings
                    if (String.IsNullOrWhiteSpace(this.familyName) || (this.familyName.Equals(Human.defaultLastName))) this.familyName = String.Empty;
                    if (String.IsNullOrWhiteSpace(this.firstName) || (this.firstName.Equals(Human.defaultFirstName))) this.firstName = String.Empty;
                    if (String.IsNullOrWhiteSpace(this.middleName) || (this.middleName.Equals(Human.defaultMiddleName))) this.middleName = String.Empty;



                    // Reset if read_only, but keep the pointer correct.
                    if (old != null)
                    {
                        if (this.familyName_readonly)
                        {
                            this.FamilyName = old.FamilyName;
                            this.Character.FamilyName = old.FamilyName;
                        }
                        if (this.firstName_readonly)
                        {
                            this.FirstName = old.FirstName;
                            this.Character.FirstName = old.FirstName;
                        }
                        if (this.middleName_readonly)
                        {
                            this.MiddleName = old.MiddleName;
                            this.Character.MiddleName = old.MiddleName;
                        }
                        if (this.age_readonly)
                        {
                            this.setAgeAndSpecies(old.Age);
                        }

                        if (this.colour_readonly)
                        {
                            this.character.Colour = old.Colour;
                            this.Colour = old.Colour;
                        }
                        if (this.hasVagina_readonly)
                        {
                            this.character.HasVagina = old.HasVagina;
                            this.HasVagina = old.HasVagina;
                        }
                        if (this.height_readonly)
                        {
                            this.character.Height = old.Height;
                            this.Height = old.Height;
                        }
                        if (this.isAnIt_readonly)
                        {
                            this.character.IsAnIt = old.IsAnIt;
                            this.IsAnIt = old.IsAnIt;
                        }
                        if (this.isMany_readonly)
                        {
                            this.character.IsMany = old.IsMany;
                            this.IsMany = old.IsMany;

                        }
                        if (this.perspective_readonly)
                        {
                            this.character.Perspective = old.Perspective;
                            this.Perspective = this.character.Perspective;
                        }
                        if (this.size_readonly)
                        {
                            this.character.Size = old.Size;
                            this.Size = old.Size;
                        }
                    }
                    

                } // end of != null
                
            }
        }

        //public void setPerspective(int perspective)
        //{
        //    this.perspective = perspective;
        //}

        //public int getPerspective()
        //{
        //    return this.perspective;

        //}

        //public UserCharacter getUserModifiedCharacter()
        //{
        //    return this._userCharacter;
        //}

        //public void setUserModifiedCharacter(UserCharacter uc)
        //{
        //    if (uc != null)
        //    {
        //        this._userCharacter = uc;

        //        this.pronouns = new Pronouns(uc.pronouns.selectedPronoun);
        //    }
        //}

        //public UserCharacter UserModifiedCharacter
        //{
        //    get {
        //        if (this._userCharacter != null)
        //        {
        //            this._userCharacter.colour = this.Colour;
        //            this._userCharacter.age = this.Age;
        //            this._userCharacter.isAnIt = this.IsAnIt;
        //            this._userCharacter.isMany = this.IsMany;
        //            //if (this.perspective != this._userCharacter.perspectiveId)
        //            //{
        //            //    this.perspective = this._userCharacter.perspectiveId;
        //            //}
        //            this._userCharacter.PerspectiveId = this.perspective;
        //            this._userCharacter.weight = this.Weight;
        //            this._userCharacter.height = this.Height;
        //            this._userCharacter.hasVagina = this.HasVagina;
        //            this._userCharacter.familyName = this.familyName;
        //            this._userCharacter.middleName = this.MiddleName;
        //            this._userCharacter.firstName = this.FirstName;
        //            return this._userCharacter;
        //        }
        //        else
        //        {
        //            this._userCharacter = new UserCharacter();
        //            this._userCharacter.colour = this.Colour;
        //            this._userCharacter.age = this.Age;
        //            this._userCharacter.isAnIt = this.IsAnIt;
        //            this._userCharacter.isMany = this.IsMany;
        //            this._userCharacter.PerspectiveId = this.perspective;
        //            this._userCharacter.weight = this.Weight;
        //            this._userCharacter.height = this.Height;
        //            this._userCharacter.hasVagina = this.HasVagina;
        //            this._userCharacter.familyName = this.familyName;
        //            this._userCharacter.middleName = this.MiddleName;
        //            this._userCharacter.firstName = this.FirstName;
        //            return this._userCharacter;
        //        }
        //    }
        //    set
        //    {
        //        if (value != null)
        //        {

        //            this._userCharacter = value;
        //            value.belongsTo = this;
        //            this.FirstName = value.firstName;
        //            this.MiddleName = value.middleName;
        //            this.familyName = value.familyName;
        //            this.HasVagina = value.hasVagina;
        //            this.Height = value.height;
        //            this.Weight = value.weight;
        //            this.perspective = value.PerspectiveId;
        //            this.IsMany = value.isMany;
        //            this.IsAnIt = value.isAnIt;
        //            this.Age = value.age;
        //            this.Colour = value.colour;

        //        }
        //    }
        //}



        

        private List<Registrable<ActionParser>> traits = new List<Registrable<ActionParser>>();
        public List<Trait> Traits
        {
            get
            {
                List<Trait> list = new List<Trait>();
                foreach (var item in this.traits)
                {
                    list.Add(item as Trait);
                }
                return list;
            }
            set
            {
                if (value == null) return;
                foreach (var item in value)
                {
                    this.addTrait(item);
                }
            }
        }


        /// <summary>
        /// Does not update the human properties.
        /// Use "updateHumanWithCharacter" method to do so.
        /// </summary>
        /// <param name="character"></param>
        public void setCharacterByReference(Character character)
        {
            if (character == null) return;
            this.character = character;
        }

        public void updateHumanWithCharacter()
        {
            // sets all the human shit 
            this.Character = this.character;
        }
        

        public int Perspective
        {
            get {

                //if (this._userCharacter != null)
                //{
                //    this.perspective = this._userCharacter.PerspectiveId;
                //    return this._userCharacter.PerspectiveId;
                //}

                if (this.character == null) return this.perspective;
                return this.character.Perspective;

                //return this.perspective;

                //if (this.perspective > 0 && this.perspective < 4)
                //{
                //    this.perspective = this._userCharacter.perspectiveId;
                //    return this.perspective;
                //}

                //return 3;// Third person default for stories. 
            }
            set {

                if (value >= 3) perspective = 3;
                else if (value <= 1) perspective = 1;
                else perspective = value;

                if (this.character != null)
                {
                    this.character.Perspective = this.perspective;
                }

                

                //if (this._userCharacter != null)
                //{
                //    this._userCharacter.PerspectiveId = this.perspective;
                //}
            }
        }


        public bool IsAnIt
        {
            get;
            set;
        } = false;

        public void addTrait(Trait trait)
        {
            if (trait != null)
            {
                if (!this.traits.Contains(trait))
                {
                    this.traits.Add(trait as Registrable<ActionParser>);
                    this.register(trait.getId(), trait as ActionParser, this.registry);
                    this.traits.Sort();
                }

            }
        }

        public void mergeTrait(Trait trait)
        {
            if (trait != null)
            {
                if (!this.traits.Contains(trait))
                {
                    this.traits.Add(trait as Registrable<ActionParser>);
                    this.register(trait.getId(), trait as ActionParser, this.registry);
                    this.traits.Sort();
                }
                else
                {
                    Trait found = this.traits[this.traits.IndexOf(trait)] as Trait;
                    found.combine(trait);
                    this.traits.Sort();
                }

            }
        }

        public List<Registrable<ActionParser>> getTraitsList()
        {
            List<Registrable<ActionParser>> list = new List<Registrable<ActionParser>>();
            foreach (Registrable<ActionParser> t in this.traits)
            {
                list.Add(t);
            }

            return list;
        }

        public List<Trait> getTraitsAsTraitsList()
        {
            List<Trait> list = new List<Trait>();
            foreach (Registrable<ActionParser> t in this.traits)
            {
                list.Add(t as Trait);
            }

            return list;
        }

        public Dictionary<string, Registrable<ActionParser>> getTraitsAsTraitsDictionary()
        {
            Dictionary<string, Registrable<ActionParser>> dictionary = new Dictionary<string, Registrable<ActionParser>>();
            foreach (Registrable<ActionParser> t in this.traits)
            {
                dictionary.Add(t.getFullId(), t);
            }

            return dictionary;
        }

        public void clearTraits()
        {
            this.traits.Clear();
            this.registry.Clear();
            this.selfRegisterAll();
        }

        //public override void selfRegisterAll()
        //{

        //}

        public void removeTrait(string scriptId)
        {
            Trait trait = null;
            foreach (var t in this.getTraitsAsTraitsList())
            {
                if (t.getId().Equals(scriptId))
                {
                    trait = t;
                    break;
                }
            }

            if (trait != null)
            {
                this.removeTrait(trait);
            }
        }

        public bool removeTrait(Trait trait)
        {
            if (trait != null)
            {
                this.traits.Remove(trait as Registrable<ActionParser>);
                try
                {
                    this.registry.Remove(trait.getId());
                    return true;
                }
                catch (Exception)
                {

                    Console.WriteLine(trait.getName() + " does not exist in the registery.");
                    return true;
                }
            }
            return false;
        }

        private List<Registrable<ActionParser>> garments = new List<Registrable<ActionParser>>();

        public List<Garment> Garments
        {
            get
            {
                List<Garment> list = new List<Garment>();
                foreach (var item in this.garments)
                {
                    list.Add(item as Garment);
                }
                return list;
            }
            set
            {
                if (value == null) return;
                foreach (var item in value)
                {
                    this.addGarment(item);
                }
            }
        }

        public void addGarment(Garment garment)
        {
            if (garment != null)
            {
                if (!this.garments.Contains(garment))
                {
                    this.garments.Add(garment as Registrable<ActionParser>);
                    this.register(garment.getId(), garment as ActionParser, this.registry);
                    this.garments.Sort();
                }
            }
        }


        public void mergeGarment(Garment garment)
        {
            if (garment != null)
            {
                if (!this.garments.Contains(garment))
                {
                    this.garments.Add(garment as Registrable<ActionParser>);
                    this.register(garment.getId(), garment as ActionParser, this.registry);
                    this.garments.Sort();
                }
                else
                {
                    Garment found = this.garments[this.garments.IndexOf(garment)] as Garment;
                    found.combine(garment);
                    this.garments.Sort();
                }

            }
        }

        public List<Registrable<ActionParser>> getGarmentsList()
        {
            List<Registrable<ActionParser>> list = new List<Registrable<ActionParser>>();
            foreach (Registrable<ActionParser> t in this.garments)
            {
                list.Add(t);
            }

            return list;
        }

        public List<Garment> getGarmentsAsGarmentList()
        {
            List<Garment> list = new List<Garment>();
            foreach (Registrable<ActionParser> t in this.garments)
            {
                list.Add(t as Garment);
            }

            return list;
        }

        public void clearGarments()
        {
            this.garments.Clear();
            this.registry.Clear();
            this.selfRegisterAll();
        }

        public void removeGarment(Garment garment)
        {
            if (garment != null)
            {
                this.garments.Remove(garment as Registrable<ActionParser>);
                try
                {
                    this.registry.Remove(garment.getId());
                }
                catch (Exception)
                {
                    Console.WriteLine(garment.getName() + " does not exist in the registery.");
                }
            }
        }

        #region Scripts
        private ListDrawString scripts = new ListDrawString(100);
        public List<string> Scripts
        {
            get
            {
                //List<string> x = new List<string>();

                //foreach (string s in aliases)
                //{
                //    x.Add(s);
                //}
                //return x;
                this.scripts.replenish();
                return this.scripts;
            }
            set
            {

                scripts = new ListDrawString(100, value);
            }
        }
        public void addScripts(string scripts)
        {
            addStringToList(this.scripts, scripts);
        }
        public void addScripts(string[] scripts)
        {
            addStringsToList(this.scripts, scripts);
        }
        public string[] getScripts()
        {
            return getList(this.scripts);
        }

        #endregion

        private string firstName = Human.defaultFirstName;
        public string FirstName
        {
            get 
            {
                if (String.IsNullOrWhiteSpace(this.firstName)) this.firstName = "";
                if (this.firstName == Human.defaultFirstName) return "";
                return this.firstName; 
            }
            set {
                firstName = value;
                // Name is the property being set in nameChanged, and it's located in the Matter class, therefore justifying doing it here.
                nameChanged();
            }
        }
        private string middleName = Human.defaultMiddleName;
        public string MiddleName
        {
            get {
                if (String.IsNullOrWhiteSpace(this.middleName)) this.middleName = "";
                if (this.middleName == Human.defaultMiddleName) return "";
                return this.middleName;
            }
            set
            {
                //if (String.IsNullOrEmpty(value.Trim())) throw new Exception("Cannot have an empty MiddleName.");
                middleName = value;
                nameChanged();
            }
        }

        private string familyName = Human.defaultLastName;
        public string FamilyName
        {
            get 
            {
                if (String.IsNullOrWhiteSpace(this.familyName)) this.familyName = "";
                if (this.familyName == Human.defaultLastName) return "";
                return familyName; 
            }
            set
            {
                //if (String.IsNullOrEmpty(value.Trim())) throw new Exception("Cannot have an empty FamilyName.");
                familyName = value;
                nameChanged();
            }
        }
        public override ActionParser deepCopy()
        {
            Human h = new Human();
            this.copy(h);
            return h as ActionParser;
        }

        protected override Matter copy(Matter m)
        {
            //Console.WriteLine(m.Name + " = Human accessed");
            Human h = m as Human;
            h.familyName = this.familyName;
            h.firstName = this.firstName;
            h.middleName = this.middleName;
            h.HasVagina = this.HasVagina;
            h.perspective = this.perspective;
            h.IsAnIt = this.IsAnIt;
            h.IsMany = this.IsMany;
            h.Scripts = this.Scripts;

            foreach (Garment g in this.garments)
            {
                h.garments.Add(g.deepCopy() as Garment);
            }

            foreach (Trait t in this.traits)
            {
                h.traits.Add(t.deepCopy() as Trait);
            }

            return base.copy(h);

        }

        private void nameChanged()
        {
            this.Name = String.Format("{0} {1} {2}", this.FirstName, this.MiddleName, this.FamilyName);
        }

        public void setAgeAndSpecies(int age)
        {
            this.Age = age;
            if (String.IsNullOrWhiteSpace(this.Spieces) || (this.Spieces.ToLower().Equals("girl") || this.Spieces.ToLower().Equals("boy") || this.Spieces.ToLower().Equals("teenager") || this.Spieces.ToLower().Equals("man") || this.Spieces.ToLower().Equals("woman") || this.Spieces.ToLower().Equals(Human.defaultSpecies)))
            {
                if (this.HasVagina)
                {
                    if (age < 13)
                    {
                        this.Spieces = "girl";
                    }
                    else if (age >= 13 && age < 20)
                    {
                        this.Spieces = "teenager";
                    }
                    else if (age >= 20)
                    {
                        this.Spieces = "woman";
                    }
                }
                else
                {
                    if (age < 13)
                    {
                        this.Spieces = "boy";
                    }
                    else if (age >= 13 && age < 20)
                    {
                        this.Spieces = "teenager";
                    }
                    else if (age >= 20)
                    {
                        this.Spieces = "man";
                    }
                }
            }
        }

        public override ActionParser getClone()
        {

            DBHuman dbHuman = new DBHuman();
            //DBMatter dbMatter = new DBMatter();

            //dbMatter = base.getDBObejct(dbMatter) as DBMatter;

            dbHuman = this.getDBObejct(dbHuman) as DBHuman;
            Human h = new Human();
            h.setFromDBObject(dbHuman);

            //try
            //{
            //    h.UserModifiedCharacter = this.UserModifiedCharacter.getClone();
            //}
            //catch (Exception ex)
            //{

            //    //throw new Exception("The UserModifiedCharacter must be chosen.");
            //    h.setUserModifiedCharacter(new UserCharacter(this.userId, this.Character));
            //}

            try
            {
                h.Character = this.Character.getClone();

            }
            catch (Exception ex)
            {

                //throw new Exception("The base character must be chosen.");
                h.Character = new Character(this.userId, r.Next(int.MaxValue).ToString(), this.FirstName, this.MiddleName, this.FamilyName, this.HasVagina);

            }

            // must set attributes that the character clone overrides.
            h.setAgeAndSpecies(this.Age);
            h.Spieces = this.Spieces;
            h.Colour = this.Colour;
            h.Size = this.Size;
            h.Height = this.Height;
            h.Weight = this.Weight;

            foreach (Garment g in this.garments)
            {
                h.garments.Add(g.getClone() as Garment);
            }

            foreach (Trait t in this.traits)
            {
                h.traits.Add(t.getClone() as Trait);
            }

            h.isPov = this.isPov;
            h.setPronoun(this.pronouns.selectedPronoun);

            return h;
            //return this.deepCopy();
            //return (ActionParser)Newtonsoft.Json.JsonConvert.DeserializeObject<Human>(this.getJson());
        }

        public static Human ObjectFromJson(string json) {
            try
            {
                Human h = Newtonsoft.Json.JsonConvert.DeserializeObject<Human>(json);

                return h;

            }
            catch (Exception)
            {

                return null;
            }
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            if (!String.IsNullOrEmpty(this.FamilyName)) info.AddValue("FirstName", this.FirstName);
            if (!String.IsNullOrEmpty(this.MiddleName)) info.AddValue("MiddleName", this.MiddleName);
            if (!String.IsNullOrEmpty(this.FamilyName)) info.AddValue("FamilyName", this.FamilyName);

            if (this.Character != null) info.AddValue("Character", this.Character);
            //if (this.UserModifiedCharacter != null) info.AddValue("UserCharacter", this.UserModifiedCharacter);
            //else
            //{
            //    if(this.Character != null) this.setUserModifiedCharacter(new UserCharacter(-1, this.Character, this.pronouns));
            //}
            info.AddValue("charactersId", this.charactersId);

            info.AddValue("HasVagina", this.HasVagina);
            info.AddValue("Perspective", this.Perspective);
            info.AddValue("IsAnIt", this.IsAnIt);

            info.AddValue("hasVagina_readonly", this.hasVagina_readonly);
            info.AddValue("familyName_readonly", this.familyName_readonly);
            info.AddValue("firstName_readonly", this.firstName_readonly);
            info.AddValue("isAnIt_readonly", this.isAnIt_readonly);
            info.AddValue("isMany_readonly", this.isMany_readonly);
            info.AddValue("middleName_readonly", this.middleName_readonly);
            info.AddValue("perspective_readonly", this.perspective_readonly);
            info.AddValue("scripts_readonly", this.scripts_readonly);


            if (this.traits.Count > 0) info.AddValue("Traits", this.traits);

            if (this.garments.Count > 0) info.AddValue("Garments", this.garments);
            if (this.scripts.Count > 0) info.AddValue("Scripts", this.scripts);

        }

        public override int CompareTo(object obj)
        {
            if (obj == null)
                return -1;
            if (!(obj is Human))
                return -1;
            if (obj.Equals(this))
                return 0;

            //string formatter = "{0}, {1} {2}";
            //string thisObject = String.Format(formatter, this.FamilyName, this.FirstName, this.MiddleName);
            //string otherObject = String.Format(formatter, ((Human)obj).FamilyName, ((Human)obj).FirstName, ((Human)obj).MiddleName);

            string formatter = "{0}, {1} {2}";
            string thisObject = String.Format(formatter, this.FirstName, this.FamilyName, this.MiddleName);
            string otherObject = String.Format(formatter, ((Human)obj).FirstName, ((Human)obj).FamilyName, ((Human)obj).MiddleName);


            return thisObject.CompareTo(otherObject); ;
        }

        protected override void setClassConstants()
        {
            this.Spieces = "Human Being";
        }

        //protected override string tripleDollarSign(int adjectiveCount = 1)
        //{
        //    string adjectives = this.chainedDraws(adjectiveCount, Matter.multipleAdjectivesSeperator, this.adjectives , Matter.multipleAdjectivesFinalizer);
        //    if (String.IsNullOrWhiteSpace(adjectives)) return this.interpret("$", tense);
        //    string interpretedFailsafe = this.interpret("N", tense);
        //    return "the " + this.drawNoConflict(adjectives, this.aliases, tense, interpretedFailsafe);
        //}

        //protected override string tripleAtSign(int adjectiveCount = 1)
        //{
        //    string adjectives = this.chainedDraws(adjectiveCount, Matter.multipleAdjectivesSeperator, this.adjectives, Matter.multipleAdjectivesFinalizer);
        //    if (String.IsNullOrWhiteSpace(adjectives)) return this.interpret("@", tense);
        //    string interpretedFailsafe = this.interpret("N", tense);
        //    return "the " + this.drawNoConflict(adjectives, this.aliases, tense, interpretedFailsafe);
        //}


        protected override void setDelegates()
        {
            base.setDelegates();
            MethodPackage.Method method = delegate (string methodName, List<string> parameters)
            {
                return "";
            };
            var d = getFreshParameterList();


            addDelegate("SC", "", null, (x, y) => { return this.drawFromList(this.scripts, ""); });
            addDelegate("SC-N", "", null, (x, y) => { return this.drawFromList(this.scripts, "", linear: true); });
            addDelegate("SC-A", "", null, (x, y) => { return this.scripts.getAssociated(); });
            addDelegate("SC-F", "", null, (x, y) => { return this.scripts.getAssociated(ListDrawString.DrawingStyle.ForcedSublist); });
            addDelegate("SC-L", "", null, (x, y) => { return this.scripts.LastSelected; });

            addDelegate("FN", "", null, (x, y) => { return this.FirstName; });
            addDelegate("MN", "", null, (x, y) => { return this.MiddleName; });
            addDelegate("LN", "", null, (x, y) => { return this.FamilyName; });
            addDelegate("PP", "", null, (x, y) => { return this.getPossesivePronoun(); });
            addDelegate("PPP", "", null, (x, y) => {
                if (this.perspective == 3)
                {
                    if (this.firstName.EndsWith("s")) return this.firstName + "'";
                    return this.firstName + "'s";
                }
                else return this.getPossesivePronoun();

            });
            addDelegate("SP", "", null, (x, y) => { return this.getSubjectPronoun(); });
            addDelegate("SPP", "", null, (x, y) => {
                return (this.perspective == 3) ? this.firstName : this.getSubjectPronoun();
            });
            addDelegate("OP", "", null, (x, y) => { return this.getObjectPronouns(); });
            addDelegate("OPP", "", null, (x, y) => { return (this.perspective == 3) ? this.firstName : this.getObjectPronouns(); });
            addDelegate("PAP", "", null, (x, y) => {
                if (this.perspective == 3)
                {
                    if (this.firstName.EndsWith("s")) return this.firstName + "'";
                    return this.firstName + "'s";
                }
                else return this.getPossessiveAdjectives();
            });
            addDelegate("POV", "", null, (x, y) => { return this.perspective.ToString(); });

            d = getFreshParameterList();
            d.Add(new MethodPackage.Parameter("Codes", "A - insert Adjective, I - get Id, C - get Color, F - fabric", typeof(string)));
            addDelegate("RANDOMGARMENT", "", d, (x, y) => { return getRandomGarment(y[0]); });

            d = getFreshParameterList();
            d.Add(new MethodPackage.Parameter("Codes", "The letters of the operations to apply to the random draw. M for possessive, R for random between possessive and article, M-R-ID-A-S-K ", typeof(string)));
            d.Add(new MethodPackage.Parameter("GroupId", "The group id of the set of traits that will be subject to a random draw.", typeof(int)));
            addDelegate("RANDOMTRAIT", "Get a random trait given the groupid supplied.", d, (x, y) =>
            {
                return this.getRandomTrait(y[0], this.tense, MethodPackage.Parameter.getParameter(y[1]).getValue());
            });

            method = (x, y) =>
            {
                string theSex = "";
                if (this.IsAnIt) theSex = "thing";
                else if (this.HasVagina) theSex = "female";
                else theSex = "male";

                if (this.IsMany) theSex += "s";
                return theSex;
            };
            d = getFreshParameterList();
            addDelegate("SEXE", "Male, female, or thing", null, method);
            addDelegate("GENDER", "Male, female, or thing", null, method);
            addDelegate("CHILD", "Boy, Girl, thing", null, (x, y) =>
            {
                string child = "";
                if (this.IsAnIt) child = "thing";
                else if (this.HasVagina) child = "girl";
                else child = "boy";
                if (this.IsMany) child += "s";
                return child;
            });
            d = getFreshParameterList();
            addDelegate("ADULT", "Man, Woman, thing", null, (x, y) =>
            {
                string adult = "";
                if (this.IsAnIt)
                {
                    adult = "thing";
                    return adult;
                }
                else if (this.HasVagina) adult = "woman";
                else adult = "man";
                if (this.IsMany)
                {
                    if (this.HasVagina) adult = "women";
                    else adult = "men";
                }
                return adult;
            });
            d = getFreshParameterList();
            addDelegate("WE", "Returns a subjective pronoun that includes this human to at least one other person. Set this person at [me.other]", null, (x, y) =>
            {
                var other = this.getOther();
                Human many = null;
                if (other == null) return this.getSubjectPronoun();
                if (!(other is Human)) return this.getPossessiveAdjectives() + " " + other.getName();
                many = getMeAsMany();
                return many.getSubjectPronoun();
            });
            d = getFreshParameterList();
            d.Add(new MethodPackage.Parameter("ID Code", "The code of the human object to use as 'other'", typeof(string)));
            addDelegate("WE", "Returns a subjective pronoun that includes this human to at least one other person. Set this person as parameter", d , (x, y) =>
            {
                var other = this.getOther();
                Human many = null;
                var param = MethodPackage.Parameter.getParameter(y[0]).getValue();
                if ((String.IsNullOrWhiteSpace(param)))
                {
                    string result = "ERROR not programmed";
                }
                if (other == null) return this.getSubjectPronoun();
                if (!(other is Human)) return this.getPossessiveAdjectives() + " " + other.getName();
                many = getMeAsMany();
                return many.getSubjectPronoun();
            });
            d = getFreshParameterList();
            addDelegate("US", "Returns an objective pronoun that includes this human to at least one other person. Set this person at [me.other]", null, (x, y) =>
            {
                var other = this.getOther();
                Human many = null;
                if (other == null) return this.getObjectPronouns();
                if (!(other is Human)) return this.getPossessiveAdjectives() + " " + other.getName();
                many = getMeAsMany();
                return many.getObjectPronouns();
            });
            d = getFreshParameterList();
            addDelegate("OUR", "", null, (x, y) =>
            {
                var other = this.getOther();
                Human many = null;
                if (other == null) return this.getPossessiveAdjectives();
                if (!(other is Human)) return this.getPossessiveAdjectives() + " " + other.getName();
                many = getMeAsMany();
                return many.getPossessiveAdjectives();
            });
            addDelegate("OURS", "", null, (x, y) =>
            {
                var other = this.getOther();
                Human many = null;
                if (other == null) return this.getPossesivePronoun();
                if (!(other is Human)) return this.getPossessiveAdjectives() + " " + other.getName();
                many = getMeAsMany();
                return many.getPossesivePronoun();
            });
            addDelegate("OURSELVES", "", null, (x, y) =>
            {
                var other = this.getOther();
                Human many = null;
                if (other == null) return this.getReflexivePronouns();
                if (!(other is Human)) return this.getPossessiveAdjectives() + " " + other.getName();
                many = getMeAsMany();
                return many.getReflexivePronouns();
            });
            addDelegate("DA", "", null, (x, y) =>
            {
                if (this.IsMany)
                    if (this.getPerspectiveAsMany() < 3)
                        return "these";
                    else
                        return "those";
                else
                {
                    int daPerspective = this.getPerspectiveAsMany();
                    if (daPerspective == 3)
                    {
                        return "that";
                    }
                    else if (daPerspective == 2)
                    {
                        return "that";
                    }
                    else if (daPerspective == 1)
                    {
                        return "this";
                    }

                }
                return "this";
            });
            addDelegate("DACLOSE", "", null, (x, y) => { return (this.IsMany) ? "these" : "this"; });
            addDelegate("DAFAR", "", null, (x, y) => { return (this.IsMany) ? "those" : "that"; });
            addDelegate("AOP", "", null, (x, y) => { return (this.perspective == 3) ? "there" : "here"; });
            addDelegate("AOPS", "", null, (x, y) => { return (this.perspective == 3) ? "there" : "here"; });
            addDelegate("AOPO", "", null, (x, y) => { return (this.perspective == 3) ? "here" : "there"; });
            addDelegate("THEKSP", "", null, (x, y) => { return (this.perspective == 3) ? "the " + this.interpret("YAR", tense) : this.getSubjectPronoun(); });
            addDelegate("THEKOP", "", null, (x, y) => { return (this.perspective == 3) ? "the " + this.interpret("YAR", tense) : this.getObjectPronouns(); });
            addDelegate("THEKPA", "", null, (x, y) => { return (this.perspective == 3) ? "the " + this.interpret("YAR", tense) + "'s" : this.getPossessiveAdjectives(); });
            addDelegate("THEKPP", "", null, (x, y) => { return (this.perspective == 3) ? "the " + this.interpret("YAR", tense) + "'s" : this.getPossesivePronoun(); });
            addDelegate("THEKRP", "", null, (x, y) => { return (this.perspective == 3) ? "the " + this.interpret("YAR", tense) : this.getReflexivePronouns(); });

            addDelegate("DAKSP", "", null, (x, y) => { return (this.perspective == 3) ? this.interpret("DA", tense) + " " + this.interpret("YAR", tense) : this.getSubjectPronoun(); });
            addDelegate("DAKOP", "", null, (x, y) => { return (this.perspective == 3) ? this.interpret("DA", tense) + " " + this.interpret("YAR", tense) : this.getObjectPronouns(); });
            addDelegate("DAKPA", "", null, (x, y) => { return (this.perspective == 3) ? this.interpret("DA", tense) + " " + this.interpret("YAR", tense) + "'s" : this.getPossessiveAdjectives(); });
            addDelegate("DAKPP", "", null, (x, y) => { return (this.perspective == 3) ? this.interpret("DA", tense) + " " + this.interpret("YAR", tense) + "'s" : this.getPossesivePronoun(); });
            addDelegate("DAKRP", "", null, (x, y) => { return (this.perspective == 3) ? this.interpret("DA", tense) + " " + this.interpret("YAR", tense) : this.getReflexivePronouns(); });

            addDelegate("DAFARKSP", "Demonstrative Adjective from the away perspective, with a YAR call.", null, (x, y) => { return (this.perspective == 3) ? this.interpret("DAFAR", tense) + " " + this.interpret("YAR", tense) : this.getSubjectPronoun(); });
            addDelegate("DAFARKOP", "Demonstrative Adjective from the away perspective, with a YAR call.", null, (x, y) => { return (this.perspective == 3) ? this.interpret("DAFAR", tense) + " " + this.interpret("YAR", tense) : this.getObjectPronouns(); });
            addDelegate("DAFARKPA", "Demonstrative Adjective from the away perspective, with a YAR call.", null, (x, y) => { return (this.perspective == 3) ? this.interpret("DAFAR", tense) + " " + this.interpret("YAR", tense) + "'s" : this.getPossessiveAdjectives(); });
            addDelegate("DAFARKPP", "Demonstrative Adjective from the away perspective, with a YAR call.", null, (x, y) => { return (this.perspective == 3) ? this.interpret("DAFAR", tense) + " " + this.interpret("YAR", tense) + "'s" : this.getPossesivePronoun(); });
            addDelegate("DAFARKRP", "Demonstrative Adjective from the away perspective, with a YAR call.", null, (x, y) => { return (this.perspective == 3) ? this.interpret("DAFAR", tense) + " " + this.interpret("YAR", tense) : this.getReflexivePronouns(); });

            addDelegate("FSP", "stands for fact subject perspective", null, (x, y) => { return ((this.IsAnIt) ? "that " : "who ") + this.drawWho(); });
            addDelegate("FOP", "stands for fact Object perspective", null, (x, y) => { return ((this.IsAnIt) ? "that " : "whom ") + this.drawWho(); });
            addDelegate("ISMANY", "", null, (x, y) => { return ((this.IsMany) ? "true" : "false"); });
            addDelegate("CLOTH", "", null, (x, y) =>
            {
                var garm = this.getLastRandomlySelectedGarment();
                if (garm == null) return "ERR: NO CLOTH!";
                return garm.interpret("$", tense);
            });
            addDelegate("MYCLOTH", "", null, (x, y) =>
            {
                var garm1 = this.getLastRandomlySelectedGarment();
                if (garm1 == null) return "ERR: NO CLOTH!";
                return this.interpret("PAP", tense) + " " + garm1.interpret("YSR", tense);
            });

            #region Overwrites the matter versions
            #region $@+%
            //==========================================================================================================
            addDelegate("@", "Objective noun phrase if the character is in the third person. Else returns a standard objective pronoun. (I, you)", null, (x, y) =>
            {
                if (this.perspective != 3) return this.getObjectPronouns();
                if (willDraw(this.AliasPercentage))
                {
                    string da = "the";
                    string yar = this.interpret("yar", tense);
                    if (!String.IsNullOrWhiteSpace(yar)) return da + " " + yar; else return this.interpret("op", tense);
                }
                else
                {
                    return "the " + this.interpret("k-a", tense);
                }
            });
            //==========================================================================================================
            addDelegate("$", "Something", null, (x, y) =>
            {
                if (this.perspective != 3) return this.getSubjectPronoun();
                if (willDraw(this.AliasPercentage))
                {
                    string da = "the"; //this.interpret("da", tense);
                    string yar = this.interpret("yar", tense);
                    if (!String.IsNullOrWhiteSpace(yar)) return da + " " + yar; else return this.interpret("sp", tense);
                }
                else
                {
                    return "the " + this.interpret("k-a", tense);
                }
            });
            //==========================================================================================================
            addDelegate("+", "Something", null, (x, y) =>
            {
                if (this.perspective != 3) return this.getPossessiveAdjectives();
                if (willDraw(this.AliasPercentage))
                {
                    string yar = this.interpret("yar", tense);
                    if (!String.IsNullOrWhiteSpace(yar))
                    {
                        if (yar.EndsWith("s")) return "the " + yar + "'";
                        return "the " + yar + "'s";
                    }

                    else return this.interpret("pa", tense);
                }
                else
                {
                    string moniker = this.interpret("k-a", tense); moniker += (moniker.EndsWith("s")) ? "'" : "'s";
                    return "the " + moniker;
                    //return "the " + this.interpret("k-a", tense) + "'s";
                }
            });
            //==========================================================================================================
            addDelegate("%", "Something", null, (x, y) =>
            {
                if (this.perspective != 3) return this.getPossesivePronoun();
                if (willDraw(this.AliasPercentage))
                {
                    string yar = this.interpret("yar", tense);
                    if (!String.IsNullOrWhiteSpace(yar))
                    {
                        if (yar.EndsWith("s")) return "the " + yar + "'";
                        return "the " + yar + "'s";
                    }
                    else return this.interpret("pp", tense);
                }
                else
                {
                    string moniker = this.interpret("k-a", tense); moniker += (moniker.EndsWith("s")) ? "'" : "'s";
                    return "the " + moniker;
                    //return "the " + this.interpret("k-a", tense) + "'s";
                }
            });
            #endregion

            #region $@+% With Parameter
            //==========================================================================================================
            d = getFreshParameterList(); d.Add(new MethodPackage.Parameter("Adjective Count", "The amount of adjectives to be drawn.", typeof(int)));
            addDelegate("@", "Objective noun phrase if the character is in the third person. Else returns a standard objective pronoun. (I, you)", d, (x, y) =>
            {
                if (this.perspective != 3) return this.getObjectPronouns();
                if (willDraw(this.AliasPercentage))
                {
                    string da = "the";
                    string adjectives = this.chainedDraws(MethodPackage.Parameter.getParameter(y[0]).getValue(), Matter.multipleAdjectivesSeperator, getDescribingList(ListCode.Adjective), Matter.multipleAdjectivesFinalizer);
                    string moniker = this.interpret("k-a", tense);
                    if (!String.IsNullOrWhiteSpace(adjectives)) return da + " " + adjectives + " " + moniker; else return this.interpret("op", tense);
                }
                else
                {
                    return "the " + this.interpret("k-a", tense);
                }
            });
            //==========================================================================================================
            d = getFreshParameterList();
            d.Add(new MethodPackage.Parameter("Adjective Count", "The amount of adjectives to be drawn.", typeof(int)));
            addDelegate("$", "Something", d, (x, y) =>
            {
                if (this.perspective != 3) return this.getSubjectPronoun();
                if (willDraw(this.AliasPercentage))
                {
                    string da = "the";
                    string adjectives = this.chainedDraws(MethodPackage.Parameter.getParameter(y[0]).getValue(), Matter.multipleAdjectivesSeperator, getDescribingList(ListCode.Adjective), Matter.multipleAdjectivesFinalizer);
                    string moniker = this.interpret("k-a", tense);
                    if (!String.IsNullOrWhiteSpace(adjectives)) return da + " " + adjectives + " " + moniker; else return this.interpret("sp", tense);
                }
                else
                {
                    return "the " + this.interpret("k-a", tense);
                }
            });
            //==========================================================================================================
            d = getFreshParameterList();
            d.Add(new MethodPackage.Parameter("Adjective Count", "The amount of adjectives to be drawn.", typeof(int)));
            addDelegate("+", "Something", d, (x, y) =>
            {
                if (this.perspective != 3) return this.getPossessiveAdjectives();
                if (willDraw(this.AliasPercentage))
                {
                    string da = "the";
                    string adjectives = this.chainedDraws(MethodPackage.Parameter.getParameter(y[0]).getValue(), Matter.multipleAdjectivesSeperator, getDescribingList(ListCode.Adjective), Matter.multipleAdjectivesFinalizer);
                    string moniker = this.interpret("k-a", tense); moniker += (moniker.EndsWith("s")) ? "'" : "'s";
                    if (!String.IsNullOrWhiteSpace(adjectives)) return da + " " + adjectives + " " + moniker; else return this.interpret("pa", tense);
                }
                else
                {
                    string moniker = this.interpret("k-a", tense); moniker += (moniker.EndsWith("s")) ? "'" : "'s";
                    return "the " + moniker;
                }
            });
            //==========================================================================================================
            d = getFreshParameterList();
            d.Add(new MethodPackage.Parameter("Adjective Count", "The amount of adjectives to be drawn.", typeof(int)));
            addDelegate("%", "Something", d, (x, y) =>
            {
                if (this.perspective != 3) return this.getPossesivePronoun();
                if (willDraw(this.AliasPercentage))
                {
                    string da = "the";
                    string adjectives = this.chainedDraws(MethodPackage.Parameter.getParameter(y[0]).getValue(), Matter.multipleAdjectivesSeperator, getDescribingList(ListCode.Adjective), Matter.multipleAdjectivesFinalizer);
                    string moniker = this.interpret("k-a", tense); moniker += (moniker.EndsWith("s")) ? "'" : "'s";
                    if (!String.IsNullOrWhiteSpace(adjectives)) return da + " " + adjectives + " " + moniker; else return this.interpret("pp", tense);
                }
                else
                {
                    string moniker = this.interpret("k-a", tense); moniker += (moniker.EndsWith("s")) ? "'" : "'s";
                    return "the " + moniker;
                }

            });
            #endregion

            //#region $@+% With Mode
            //==========================================================================================================
            d = getFreshParameterList();
            d.Add(new MethodPackage.Parameter("Adjective Count", "The amount of adjectives to be drawn.", typeof(int)));
            d.Add(new MethodPackage.Parameter("Mode", "Lists to use. S,A,V,SA,VA,VS,SAV", typeof(string)));
            addDelegate("@", "Objective noun phrase if the character is in the third person. Else returns a standard objective pronoun. (I, you)", d, (x, y) =>
            {
                if (this.perspective != 3) return this.getObjectPronouns();
                if (willDraw(this.AliasPercentage))
                {
                    string da = "the";
                    string adjectives = this.chainedDraws(
                        MethodPackage.Parameter.getParameter(y[0]).getValue(),
                        Matter.multipleAdjectivesSeperator,
                        getDescribingList(MethodPackage.Parameter.getParameter(y[1]).getValue()),
                        Matter.multipleAdjectivesFinalizer
                        );
                    string moniker = this.interpret("k-a", tense);
                    if (!String.IsNullOrWhiteSpace(adjectives)) return da + " " + adjectives + " " + moniker; else return this.interpret("op", tense);
                }
                else
                {
                    return "the " + this.interpret("k-a", tense);
                }
            });
            //==========================================================================================================
            d = getFreshParameterList();
            d.Add(new MethodPackage.Parameter("Adjective Count", "The amount of adjectives to be drawn.", typeof(int)));
            d.Add(new MethodPackage.Parameter("Mode", "Lists to use. S,A,V,SA,VA,VS,SAV", typeof(string)));
            addDelegate("$", "Something", d, (x, y) =>
            {
                if (this.perspective != 3) return this.getSubjectPronoun();
                if (willDraw(this.AliasPercentage))
                {
                    string da = "the";
                    string adjectives = this.chainedDraws(
                        MethodPackage.Parameter.getParameter(y[0]).getValue(),
                        Matter.multipleAdjectivesSeperator,
                        getDescribingList(MethodPackage.Parameter.getParameter(y[1]).getValue()),
                        Matter.multipleAdjectivesFinalizer
                        );
                    string moniker = this.interpret("k-a", tense);
                    if (!String.IsNullOrWhiteSpace(adjectives)) return da + " " + adjectives + " " + moniker; else return this.interpret("sp", tense);
                }
                else
                {
                    return "the " + this.interpret("k-a", tense);
                }
            });
            //==========================================================================================================
            d = getFreshParameterList();
            d.Add(new MethodPackage.Parameter("Adjective Count", "The amount of adjectives to be drawn.", typeof(int)));
            d.Add(new MethodPackage.Parameter("Mode", "Lists to use. S,A,V,SA,VA,VS,SAV", typeof(string)));
            addDelegate("+", "Something", d, (x, y) =>
            {
                if (this.perspective != 3) return this.getPossessiveAdjectives();
                if (willDraw(this.AliasPercentage))
                {
                    string da = "the";
                    string adjectives = this.chainedDraws(
                        MethodPackage.Parameter.getParameter(y[0]).getValue(),
                        Matter.multipleAdjectivesSeperator,
                        getDescribingList(MethodPackage.Parameter.getParameter(y[1]).getValue()),
                        Matter.multipleAdjectivesFinalizer
                        );
                    string moniker = this.interpret("k-a", tense); moniker += (moniker.EndsWith("s")) ? "'" : "'s";
                    if (!String.IsNullOrWhiteSpace(adjectives)) return da + " " + adjectives + " " + moniker; else return this.interpret("pa", tense);
                }
                else
                {
                    string moniker = this.interpret("k-a", tense); moniker += (moniker.EndsWith("s")) ? "'" : "'s";
                    return "the " + moniker;
                    //return "the " + this.interpret("k-a", tense) + "'s";
                }
            });
            //==========================================================================================================
            d = getFreshParameterList();
            d.Add(new MethodPackage.Parameter("Adjective Count", "The amount of adjectives to be drawn.", typeof(int)));
            d.Add(new MethodPackage.Parameter("Mode", "Lists to use. S,A,V,SA,VA,VS,SAV", typeof(string)));
            addDelegate("%", "Something", d, (x, y) =>
            {
                if (this.perspective != 3) return this.getPossesivePronoun();
                if (willDraw(this.AliasPercentage))
                {
                    string da = "the";
                    string adjectives = this.chainedDraws(
                        MethodPackage.Parameter.getParameter(y[0]).getValue(),
                        Matter.multipleAdjectivesSeperator,
                        getDescribingList(MethodPackage.Parameter.getParameter(y[1]).getValue()),
                        Matter.multipleAdjectivesFinalizer
                        );
                    string moniker = this.interpret("k-a", tense); moniker += (moniker.EndsWith("s")) ? "'" : "'s";
                    if (!String.IsNullOrWhiteSpace(adjectives)) return da + " " + adjectives + " " + moniker; else return this.interpret("pp", tense);
                }
                else
                {
                    string moniker = this.interpret("k-a", tense); moniker += (moniker.EndsWith("s")) ? "'" : "'s";
                    return "the " + moniker;
                    //return "the " + this.interpret("k-a", tense) + "'s";
                }

            });
            #endregion

            addDelegate("?", "Looks for the Human's 'conditionals' trait or garment and executes the facts list. An error returns an empty string.", null, (x, y) => { return this.getConditional(tense); });
            //==========================================================================================================
            addDelegate("$$$", "Returns $, but Ensures and SPP failsafe", null, (x, y) =>
            {
                if (this.perspective != 3) return this.getSubjectPronoun();
                return this.tripleDollarSign();
            });
            d = getFreshParameterList(); d.Add(new MethodPackage.Parameter("Adjective Count", "The amount of adjectives to be drawn.", typeof(int)));
            addDelegate("$$$", "Returns $, but Ensures and SPP failsafe. Integer parameter is the adjective count returned.", d, (x, y) =>
            {
                if (this.perspective != 3) return this.getSubjectPronoun();
                return tripleDollarSign(MethodPackage.Parameter.getParameter(y[0]).getValue());
            });
            //==========================================================================================================
            addDelegate("@@@", "Returns @, but Ensures and OPP failsafe.", null, (x, y) =>
            {
                if (this.perspective != 3) return this.getObjectPronouns();
                return this.tripleAtSign();
            });
            d = getFreshParameterList(); d.Add(new MethodPackage.Parameter("Adjective Count", "The amount of adjectives to be drawn.", typeof(int)));
            addDelegate("@@@", "Returns @, but Ensures and OPP failsafe. Integer parameter is the adjective count returned.", d, (x, y) =>
            {
                if (this.perspective != 3) return this.getObjectPronouns();
                return this.tripleAtSign(MethodPackage.Parameter.getParameter(y[0]).getValue());
            });
            //#endregion


            #region Verb Methods
            //==========================================================================================================
            d = getFreshParameterList();
            d.Add(new MethodPackage.Parameter("Infinitive", "The verb in infinitive form.", typeof(string)));
            addDelegate("V", "Something", d, (x, y) =>
            {
                var relativeTense = Verb.getTenseFromInt(this.tense);
                var gender = this.HasVagina ? Verb.Gender.female : Verb.Gender.male;
                var verb = new Verb(MethodPackage.Parameter.getParameter(y[0]).getValue(), this.IsMany, this.perspective, gender, relativeTense);
                return verb.getVerb();
            });
            //==========================================================================================================
            d = getFreshParameterList();
            d.Add(new MethodPackage.Parameter("Infinitive", "The verb in infinitive form.", typeof(string)));
            d.Add(new MethodPackage.Parameter("Relative tense", "Values accepted are -1, 0, 1, 2", typeof(int)));
            addDelegate("V", "Something", d, (x, y) =>
            {

                int secondParameter = MethodPackage.Parameter.getParameter(y[1]).getValue();
                if (secondParameter < -1 || secondParameter > 2) secondParameter = 0;
                var relativeTense = Verb.getTenseFromInt(this.tense + secondParameter);
                var gender = this.HasVagina ? Verb.Gender.female : Verb.Gender.male;
                var verb = new Verb(MethodPackage.Parameter.getParameter(y[0]).getValue(), this.IsMany, this.perspective, gender, relativeTense);
                return verb.getVerb();
            });
            //==========================================================================================================
            d = getFreshParameterList();
            d.Add(new MethodPackage.Parameter("Infinitive", "The verb in infinitive form.", typeof(string)));
            d.Add(new MethodPackage.Parameter("Relative tense", "Values accepted are -1, 0, 1, 2", typeof(int)));
            d.Add(new MethodPackage.Parameter("Adverb", "And adverb to slap into the verb phrase.", typeof(string)));
            addDelegate("V", "Something", d, (x, y) =>
            {
                int secondParameter = MethodPackage.Parameter.getParameter(y[1]).getValue();
                if (secondParameter < -1 || secondParameter > 2) secondParameter = 0;
                var relativeTense = Verb.getTenseFromInt(this.tense + secondParameter);
                //var relativeTense = Verb.getTenseFromInt(tense + MethodPackage.Parameter.getParameter(y[1]).getValue());
                var gender = this.HasVagina ? Verb.Gender.female : Verb.Gender.male;
                var verb = new Verb(MethodPackage.Parameter.getParameter(y[0]).getValue(), this.IsMany, this.perspective, gender, relativeTense, MethodPackage.Parameter.getParameter(y[2]).getValue());
                return verb.getVerb();
            });
            //d = getFreshParameterDictionary();
            //d.Add(new MethodPackage.Parameter("Infinitive", "The verb in infinitive form.", typeof(string)));
            //d.Add(new MethodPackage.Parameter("Adverb", "And adverb to slap into the verb phrase.", typeof(string)));
            //addDelegate("V", "Something", d, (x, y) =>
            //{
            //    var gender = this.hasVagina ? Verb.Gender.female : Verb.Gender.male;
            //    var verb = new Verb(y[0], this.IsMany, this.perspective, gender, Verb.getTenseFromInt(tense), y[1]);
            //    return verb.getVerb();
            //});


            //==========================================================================================================

            d = getFreshParameterList();
            d.Add(new MethodPackage.Parameter("Infinitive", "The verb in infinitive form.", typeof(string)));
            d.Add(new MethodPackage.Parameter("Adverb", "And adverb to slap into the verb phrase.", typeof(string)));
            addDelegate("V", "Something", d, (x, y) =>
            {
                var gender = this.HasVagina ? Verb.Gender.female : Verb.Gender.male;
                int parameteredRelativeTense = Verb.getIntFromString(y[1]);
                Verb verb = null;
                if (parameteredRelativeTense != -1)
                {
                    verb = new Verb(y[0], this.IsMany, this.perspective, gender, Verb.getTenseFromInt(parameteredRelativeTense));
                }
                else
                {
                    verb = new Verb(y[0], this.IsMany, this.perspective, gender, Verb.getTenseFromInt(this.tense), y[1]);
                }

                return verb.getVerb();
            });
            //==========================================================================================================
            d = getFreshParameterList();
            d.Add(new MethodPackage.Parameter("Infinitive", "The verb in infinitive form.", typeof(string)));
            d.Add(new MethodPackage.Parameter("Constant Tense", "The tense constant name in string form. Case sensitive.", typeof(string)));
            d.Add(new MethodPackage.Parameter("Adverb", "And adverb to slap into the verb phrase.", typeof(string)));
            addDelegate("V", "Something", d, (x, y) =>
            {
                var gender = this.HasVagina ? Verb.Gender.female : Verb.Gender.male;
                int parameteredRelativeTense = Verb.getIntFromString(y[1]);
                Verb verb = null;
                if (parameteredRelativeTense != -1)
                {
                    parameteredRelativeTense = Verb.getIntFromString("Continuous");

                }
                verb = new Verb(y[0], this.IsMany, this.perspective, gender, Verb.getTenseFromInt(parameteredRelativeTense), y[2]);
                return verb.getVerb();
            });

            #endregion

            //==========================================================================================================


            d = getFreshParameterList();
            d.Add(new MethodPackage.Parameter("Selection method codes", "Select a combination of K/A/ID/M/R", typeof(int)));
            d.Add(new MethodPackage.Parameter("Random Trait Group Id", "The group Id form which a random trait is selected from.", typeof(int)));
            addDelegate("RNDTRAIT", "Something", d, (x, y) => { return getRandomTrait(y[0], MethodPackage.Parameter.getParameter(y[1]).getValue()); });


            addDelegate("SP", "", null, (x, y) => { return this.getSubjectPronoun(); });

            //==========================================================================================================

            d = getFreshParameterList();
            d.Add(new MethodPackage.Parameter("Negative", "Anything other than zero is considered negative in value.", typeof(int)));
            addDelegate("COULD", "Used to replace stories that use the word 'could' with 'is able to'.", d, (x, y) =>
            {
                int negationParameter = MethodPackage.Parameter.getParameter(y[0]).getValue();
                var could = this.find("could");

                if (could != null && could is Trait)
                {
                    if (negationParameter == 0)
                    {
                        return could.interpret("K", this.tense);
                    }
                    else
                    {
                        return could.interpret("V", this.tense);
                    }
                }


                return "Failed to locate a 'could' trait.";
            });
            //==========================================================================================================

            d = getFreshParameterList();
            addDelegate("CAN", "Used to replace stories that use the word 'could' or 'can' with 'is able to'.", d, (x, y) =>
            {
                var could = this.find("could");

                if (could != null && could is Trait)
                {
                    return could.interpret("K", this.tense);
                }


                return "Failed to locate a 'could' trait.";
            });

            //==========================================================================================================

            d = getFreshParameterList();
            addDelegate("CANT", "Used to replace stories that use the word 'could not' or 'can not' with 'is not able to'.", d, (x, y) =>
            {
                var could = this.find("could");

                if (could != null && could is Trait)
                {
                    return could.interpret("V", this.tense);
                }


                return "Failed to locate a 'could' trait.";
            });



        }

        public override string interpret(string code, int tense)
        {
            return base.interpret(code, tense);
            string paramCode = code;
            code = code.ToUpper();
            if (paramCode.Length == 0) return "";


            // legacy
            //if (code.StartsWith("{") && code.EndsWith("}"))
            //{
            //    code = code.Remove(code.Length - 1, 1).Remove(0, 1);
            //}

            // ex: SC-(A1)
            // ex: SC-A(sdfs)
            //Regex regexCallListWithKey = new Regex(@"^(SC)\-A\(([a-z]|[A-Z]|\d)+\)$", RegexOptions.IgnoreCase);
            if (regexCallListWithKey.IsMatch(code.ToUpper()))
            {
                ListDrawString list = null;
                string theCode = code.ToUpper();

                // Always starts with SC for the time being
                if (code.StartsWith("SC")) {
                    list = this.scripts;

                    string listCode = theCode.Remove(0, 2);
                    listCode = listCode.Remove(listCode.Length - 1, 1);

                    return list.draw(listCode);
                }
            }

            Regex regexTrait = new Regex(@"^T\d+", RegexOptions.IgnoreCase);
            Verb.Gender gender = Verb.Gender.it;
            if (this.IsAnIt) gender = Verb.Gender.it;
            else if (this.HasVagina) gender = Verb.Gender.female;
            else gender = Verb.Gender.male;


            if (paramCode.StartsWith("v?("))
            {
                // if this person isn't the story's pov character and isn't first person perperspective.
                if (!this.isPov && !(this.perspective == 1))
                {
                    paramCode = paramCode.Remove(0, 3);
                    paramCode = "v(?" + paramCode;
                    string verbv = Verb.getVerb(paramCode, this.IsMany, this.perspective, this.IsAnIt, gender, tense);
                    if (!String.IsNullOrWhiteSpace(verbv)) return verbv;
                }
                else
                {
                    paramCode = paramCode.Remove(0, 3);
                    paramCode = "v(" + paramCode;
                    string verbv = Verb.getVerb(paramCode, this.IsMany, this.perspective, this.IsAnIt, gender, tense);
                    if (!String.IsNullOrWhiteSpace(verbv)) return verbv;
                }
            }
            string verb = Verb.getVerb(paramCode, this.IsMany, this.perspective, this.IsAnIt, gender, tense);
            if (!String.IsNullOrWhiteSpace(verb)) return verb;

            Regex regexGarmentSelections = new Regex(@"^G(ID|C|A|F){1,3}\d*$", RegexOptions.IgnoreCase);
            if (regexGarmentSelections.IsMatch(code))
            {
                return getRandomGarment(code);
            }
            // This is the Random trait part.
            else if (regexTrait.IsMatch(code))
            {
                char[] chars = code.ToCharArray();
                if (chars.Length > 1)
                {
                    string strGroupId = "";
                    int groupId = 0; // default group is called
                    Array.Reverse(chars);
                    int size = chars.Length - 1;
                    Array.Resize<char>(ref chars, size);
                    Array.Reverse(chars);
                    foreach (char myChar in chars)
                    {
                        int test = 0;
                        if (int.TryParse(myChar.ToString(), out test))
                            strGroupId += myChar.ToString();
                        else
                            break;
                    }

                    if (!String.IsNullOrEmpty(strGroupId))
                    {

                        code = code.Replace(strGroupId, "");
                        if (code.StartsWith("T"))
                        {
                            code = code.Remove(0, 1);
                        }
                    }
                    if (int.TryParse(strGroupId, out groupId))
                    {
                        return getRandomTrait(code, groupId);
                    }
                }

                return this.getRandomTrait(code, tense);
            }

            //Regex regexIsParentheses = new Regex(@"^\(\w+\)$",RegexOptions.IgnoreCase);
            Regex regexQuickMonikers = new Regex(@"^(\@|\+|\%|\$|\$\$|\@\@|\+\+|\%\%|\$\$\$|\@\@\@){1}.*$");
            if (regexQuickMonikers.IsMatch(code))
            {

                if (code.StartsWith("@@@"))
                {
                    if (this.perspective != 3) return this.getObjectPronouns();
                    return base.interpret(paramCode, tense);

                }

                if (code.StartsWith("$$$"))
                {
                    if (this.perspective != 3) return this.getSubjectPronoun();
                    return base.interpret(paramCode, tense);

                }




                // !@#$%^&*()_+
                if (code.StartsWith("$$")) return this.refOther(code, "$", new string[] { "~the", "as", "k" }, tense);
                if (code.StartsWith("@@")) return this.refOther(code, "@", new string[] { "da", "as", "k" }, tense);
                if (code.StartsWith("++")) return this.refOther(code, "+", new string[] { "wpa", "as", "k" }, tense);
                if (code.StartsWith("%%")) return this.refOther(code, "", new string[] { "~the", "as", "k", "~of", "wpp" }, tense);

                if (code.StartsWith("@"))
                {
                    if (this.perspective != 3) return this.getObjectPronouns();
                    return base.interpret(paramCode, tense);
                }

                if (code.StartsWith("$"))
                {
                    if (this.perspective != 3) return this.getSubjectPronoun();
                    return base.interpret(paramCode, tense);
                }

                if (code.StartsWith("+"))
                {
                    if (code.Length > 1)
                    {
                        code = paramCode.Remove(0, 1);
                        if (this.perspective != 3) return this.getPossessiveAdjectives();
                        return this.interpret("*" + code, tense);
                    }
                    else
                    {
                        if (this.perspective != 3) return this.getPossessiveAdjectives();
                        return base.interpret(code, tense);
                    }
                }

                if (code.StartsWith("%"))
                {
                    if (code.Length > 1)
                    {
                        code = paramCode.Remove(0, 1);
                        if (this.perspective != 3) return this.getPossesivePronoun();
                        return this.interpret("*" + code, tense);
                    }
                    else
                    {
                        if (this.perspective != 3) return this.getPossesivePronoun();
                        return base.interpret(code, tense);
                    }
                }





            }

            Human many = null;
            var other = this.getOther();

            switch (code)
            {
                case "FN":
                    return this.FirstName;
                case "MN":
                    return this.MiddleName;
                case "LN":
                    return this.FamilyName;
                case "PP":
                    return this.getPossesivePronoun();
                case "PPP":
                    if (this.perspective == 3)
                        return this.firstName + "'s";
                    else
                        return this.getPossesivePronoun();
                case "SP":
                    return this.getSubjectPronoun();
                case "SPP":
                    if (this.perspective == 3)
                        return this.firstName;
                    else
                        return this.getSubjectPronoun();
                case "OP":
                    return this.getObjectPronouns();
                case "OPP":
                    if (this.perspective == 3)
                        return this.firstName;
                    else
                        return this.getObjectPronouns();
                case "PAP":
                    if (this.perspective == 3)
                        return this.firstName + "'s";
                    else
                        return this.getPossessiveAdjectives();


                case "POV":
                    return this.perspective.ToString();
                case "SEXE":
                    string theSex = "";
                    if (this.IsAnIt) theSex = "thing";
                    else if (this.HasVagina) theSex = "female";
                    else theSex = "male";

                    if (this.IsMany) theSex += "s";
                    return theSex;
                case "GENDER":
                    string theGender = "";
                    if (this.IsAnIt) theGender = "thing";
                    else if (this.HasVagina) theGender = "female";
                    else theGender = "male";

                    if (this.IsMany) theGender += "s";
                    return theGender;
                case "CHILD":
                    string child = "";
                    if (this.IsAnIt) child = "thing";
                    else if (this.HasVagina) child = "girl";
                    else child = "boy";
                    if (this.IsMany) child += "s";
                    return child;
                case "ADULT":
                    string adult = "";
                    if (this.IsAnIt)
                    {
                        adult = "thing";
                        return adult;
                    }
                    else if (this.HasVagina) adult = "woman";
                    else adult = "man";
                    if (this.IsMany)
                    {
                        if (this.HasVagina) adult = "women";
                        else adult = "men";
                    }
                    return adult;

                case "WE":
                    if (other == null) return this.getSubjectPronoun();
                    if (!(other is Human)) return this.getPossessiveAdjectives() + " " + other.getName();
                    many = getMeAsMany();
                    return many.getSubjectPronoun();

                case "US":
                    if (other == null) return this.getObjectPronouns();
                    if (!(other is Human)) return this.getPossessiveAdjectives() + " " + other.getName();
                    many = getMeAsMany();
                    return many.getObjectPronouns();

                case "OUR":
                    if (other == null) return this.getPossessiveAdjectives();
                    if (!(other is Human)) return this.getPossessiveAdjectives() + " " + other.getName();
                    many = getMeAsMany();
                    return many.getPossessiveAdjectives();

                case "OURS":
                    if (other == null) return this.getPossesivePronoun();
                    if (!(other is Human)) return this.getPossessiveAdjectives() + " " + other.getName();
                    many = getMeAsMany();
                    return many.getPossesivePronoun();

                case "OURSELVES":
                    if (other == null) return this.getReflexivePronouns();
                    if (!(other is Human)) return this.getPossessiveAdjectives() + " " + other.getName();
                    many = getMeAsMany();
                    return many.getReflexivePronouns();

                case "DA": //demonstrative adjective
                    if (this.IsMany)
                        if (this.getPerspectiveAsMany() < 3)
                            return "these";
                        else
                            return "those";
                    else
                    {
                        int daPerspective = this.getPerspectiveAsMany();
                        if (daPerspective == 3)
                        {
                            return "that";
                        }
                        else if (daPerspective == 2)
                        {
                            return "that";
                        }
                        else if (daPerspective == 1)
                        {
                            return "this";
                        }

                    }
                    return "this";
                case "DACLOSE":
                    if (this.IsMany)
                        return "these";
                    else
                        return "this";
                case "DAFAR":
                    if (this.IsMany)
                        return "those";
                    else
                        return "that";
                case "AOP":  // adverb of place default point of view (subjective)
                    if (perspective == 3)
                        return "there";
                    else
                        return "here";
                case "AOPS":  // adverb of place subjective point of view
                    if (perspective == 3)
                        return "there";
                    else
                        return "here";
                case "AOPO":  // adverb of place objective point of view
                    if (perspective == 3)
                        return "here";
                    else
                        return "there";

                case "SCRIPT":
                    return this.drawFromList(this.scripts, "");
                case "SC":
                    return this.drawFromList(this.scripts, "");
                case "SC-N":
                    return this.drawFromList(this.scripts, "", linear: true);
                case "SC-A":
                    return this.scripts.getAssociated();
                case "SC-F":
                    return this.scripts.getAssociated(ListDrawString.DrawingStyle.ForcedSublist);
                case "SC-L":
                    return this.scripts.LastSelected;

                case "THEKSP":
                    if (this.perspective == 3)
                        return "the " + base.interpret("A", tense) + " " + base.interpret("K", tense);
                    else
                        return this.getSubjectPronoun();

                case "THEKOP":
                    if (this.perspective == 3)
                        return "the " + base.interpret("A", tense) + " " + base.interpret("K", tense);
                    else
                        return this.getObjectPronouns();

                case "THEKPA":
                    if (this.perspective == 3)
                        return "the " + base.interpret("A", tense) + " " + base.interpret("K", tense) + "'s";
                    else
                        return this.getPossessiveAdjectives();

                case "THEKPP":
                    if (this.perspective == 3)
                        return "the " + base.interpret("A", tense) + " " + base.interpret("K", tense) + "'s";
                    else
                        return this.getPossesivePronoun();

                case "THEKRP":
                    if (this.perspective == 3)
                        return "the " + base.interpret("A", tense) + " " + base.interpret("K", tense);
                    else
                        return this.getReflexivePronouns();

                case "DAKSP":
                    if (this.perspective == 3)
                        return this.interpret("DA", tense) + " " + base.interpret("A", tense) + " " + base.interpret("K", tense);
                    else
                        return this.getSubjectPronoun();

                case "DAKOP":
                    if (this.perspective == 3)
                        return this.interpret("DA", tense) + " " + base.interpret("A", tense) + " " + base.interpret("K", tense);
                    else
                        return this.getObjectPronouns();

                case "DAKPA":
                    if (this.perspective == 3)
                        return this.interpret("DA", tense) + " " + base.interpret("A", tense) + " " + base.interpret("K", tense) + "'s";
                    else
                        return this.getPossessiveAdjectives();

                case "DAKPP":
                    if (this.perspective == 3)
                        return this.interpret("DA", tense) + " " + base.interpret("A", tense) + " " + base.interpret("K", tense) + "'s";
                    else
                        return this.getPossesivePronoun();

                case "DAKRP":
                    if (this.perspective == 3)
                        return this.interpret("DA", tense) + " " + base.interpret("A", tense) + " " + base.interpret("K", tense);
                    else
                        return this.getReflexivePronouns();

                case "DAFARKSP":
                    if (this.perspective == 3)
                        return this.interpret("DAFAR", tense) + " " + base.interpret("A", tense) + " " + base.interpret("K", tense);
                    else
                        return this.getSubjectPronoun();

                case "DAFARKOP":
                    if (this.perspective == 3)
                        return this.interpret("DAFAR", tense) + " " + base.interpret("A", tense) + " " + base.interpret("K", tense);
                    else
                        return this.getObjectPronouns();

                case "DAFARKPA":
                    if (this.perspective == 3)
                        return this.interpret("DAFAR", tense) + " " + base.interpret("A", tense) + " " + base.interpret("K", tense) + "'s";
                    else
                        return this.getPossessiveAdjectives();

                case "DAFARKPP":
                    if (this.perspective == 3)
                        return this.interpret("DAFAR", tense) + " " + base.interpret("A", tense) + " " + base.interpret("K", tense) + "'s";
                    else
                        return this.getPossesivePronoun();

                case "DAFARKRP":
                    if (this.perspective == 3)
                        return this.interpret("DAFAR", tense) + " " + base.interpret("A", tense) + " " + base.interpret("K", tense);
                    else
                        return this.getReflexivePronouns();

                case "FSP": // stands for fact subject perspective
                    return ((this.IsAnIt) ? "that " : "who ") + this.drawWho();

                case "FOP": // stands for fact Object perspective
                    return ((this.IsAnIt) ? "that " : "whom ") + this.drawWho();
                case "ISMANY":
                    return ((this.IsMany) ? "true" : "false");
                case "CLOTH":
                    var garm = this.getLastRandomlySelectedGarment();
                    if (garm == null) return "ERR: NO CLOTH!";
                    return garm.interpret("$", tense);
                case "MYCLOTH":
                    var garm1 = this.getLastRandomlySelectedGarment();
                    if (garm1 == null) return "ERR: NO CLOTH!";
                    return this.interpret("PAP", tense) + " " + garm1.interpret("YSR", tense);
                case "?":
                    return this.getConditional(tense);

                default:
                    return base.interpret(paramCode, tense);
            }
        }

        #region useless Verb crap

        private Human getMeAsMany()
        {

            Human many = new Human();
            many.IsMany = true;
            many.perspective = getPerspectiveAsMany();

            return many;
        }

        protected override string getPossesivePronoun()
        {

            if (this.perspective == 3)
            {

                if (!this.IsMany)
                {
                    if (this.IsAnIt)
                    {
                        if (this.pronouns != null) return this.pronouns.Possessive;
                        return "its"; // not used
                    }
                    if (this.HasVagina)
                    {
                        return "hers";
                    }
                    else
                    {
                        return "his";
                    }
                }
                else
                {
                    return "theirs";
                }
            }
            else if (perspective == 2)
            {
                return "yours";
            }
            else
            {
                if (!this.IsMany)
                {
                    return "mine";
                }
                else
                {
                    return "ours";
                }
            }

        }

        protected override string getSubjectPronoun()
        {

            if (this.perspective == 3)
            {

                if (!this.IsMany)
                {
                    if (this.IsAnIt)
                    {
                        if (this.pronouns != null) return this.pronouns.Subject;
                        return "it";
                    }
                    if (this.HasVagina)
                    {
                        return "she";
                    }
                    else
                    {
                        return "he";
                    }
                }
                else
                {
                    return "they";
                }
            }
            else if (perspective == 2)
            {
                return "you";
            }
            else
            {
                if (!this.IsMany)
                {
                    return "I";
                }
                else
                {
                    return "we";
                }
            }
        }

        protected override string getObjectPronouns()
        {

            if (this.perspective == 3)
            {

                if (!this.IsMany)
                {
                    if (this.IsAnIt)
                    {
                        if (this.pronouns != null) return this.pronouns.Object;
                        return "it";
                    }
                    if (this.HasVagina)
                    {
                        return "her";
                    }
                    else
                    {
                        return "him";
                    }
                }
                else
                {
                    return "them";
                }
            }
            else if (perspective == 2)
            {
                return "you";
            }
            else
            {
                if (!this.IsMany)
                {
                    return "me";
                }
                else
                {
                    return "us";
                }
            }
        }

        protected override string getPossessiveAdjectives()
        {

            if (this.perspective == 3)
            {

                if (!this.IsMany)
                {
                    if (this.IsAnIt)
                    {
                        if (this.pronouns != null) return this.pronouns.Personal;
                        return "its";
                    }
                    if (this.HasVagina)
                    {
                        return "her";
                    }
                    else
                    {
                        return "his";
                    }
                }
                else
                {
                    return "their";
                }
            }
            else if (perspective == 2)
            {
                return "your";
            }
            else
            {
                if (!this.IsMany)
                {
                    return "my";
                }
                else
                {
                    return "our";
                }
            }
        }

        protected override string getReflexivePronouns()
        {
            if (this.perspective == 3)
            {

                if (!this.IsMany)
                {
                    if (this.IsAnIt)
                    {
                        if (this.pronouns != null) return this.pronouns.Reflexive;
                        return "itself";
                    }
                    if (this.HasVagina)
                    {
                        return "herself";
                    }
                    else
                    {
                        return "himself";
                    }
                }
                else
                {
                    return "themselves";
                }
            }
            else if (perspective == 2)
            {
                if (!this.IsMany)
                {
                    return "yourself";
                }
                else
                {
                    return "yourselves";
                }
            }
            else
            {
                if (!this.IsMany)
                {
                    return "myself";
                }
                else
                {
                    return "ourselves";
                }
            }
        }

        protected int getPerspectiveAsMany()
        {
            if (this.getOther() == null) return this.perspective;
            Human other = (this.getOther() as Human);
            int perspective = (int)((other.Perspective * 30) / this.perspective);
            //60/3=20; their, theirs
            //40/3=13; your, yours
            //20/3=6; our, ours
            //60/2=30; your, yours
            //40/2=20; your, yours
            //20/2=10; our, ours
            //60/1=60; our, ours
            //40/1=40; our, ours
            //20/1=20; our, ours
            if (this.perspective == 1 || other.Perspective == 1) return 1;
            if (this.perspective == 2 || other.Perspective == 2) return 2;
            return 3;
        }

        public override void selfRegisterAll()
        {
            base.selfRegisterAll();

            // register garments
            this.autoRegister(this.garments);

            // register traits
            this.autoRegister(this.traits);

            //
        }

        private string getConditional(int tense)
        {

            if (!this.isPov && this.perspective == 3)
            {

                Dictionary<string, ActionParser> dic = this.getDictionaryOfRegisteredObjects();
                try
                {
                    return dic["conditionals"].interpret("f", tense);

                }
                catch (Exception e)
                {

                }

                return "";
            }
            
            return "";
        }

        private string getRandomTrait(string code, int tense, int groupId = 0)
        {
            if (this.traits.Count == 0) return "";

            List<Trait> traitList = new List<Trait>();

            foreach (Trait trait in this.traits)
            {
                if (trait.IsUsed)
                {
                    if (groupId == -1)
                    {

                        traitList.Add(trait);
                    }
                    else
                    {
                        if (trait.GroupId == groupId) traitList.Add(trait);
                    }
                }
            }

            if (traitList.Count == 0) return "";

            Trait randomTrait = null;// (Trait)traitList[r.Next(traitList.Count)];
            string adj = "";
            string randomAdjective = "";
            if (code.IndexOf('R') > -1)
            {
                for (int i = 0; i < 100; i++)
                {
                    randomTrait = (Trait)traitList[r.Next(traitList.Count)];
                    if (randomTrait.Adjectives is ListDrawString)
                    {
                        randomAdjective = (randomTrait.Adjectives as ListDrawString).draw();
                        if (!String.IsNullOrWhiteSpace(randomAdjective))
                        {
                            break;
                        }
                    }
                }

                if (String.IsNullOrWhiteSpace(randomAdjective))
                {
                    return "";
                }
                else
                {
                    int x = (int)r.Next(100);
                    //experimental. Passing "the" every time for the moment. 
                    string ret = (x > 100) ? this.interpret("pa", tense) : "the";
                    ret += " " + randomAdjective + " " + randomTrait.interpret("k", tense);
                    return ret;

                }
            }
            if (code.IndexOf('M') > -1)
            {
                randomTrait = (Trait)traitList[r.Next(traitList.Count)];
                if (randomTrait.Adjectives is ListDrawString)
                {
                    randomAdjective = (randomTrait.Adjectives as ListDrawString).draw();
                }
                int x = (int)r.Next(100);
                string ret = this.interpret("pa", tense);
                ret += " " + randomAdjective + " " + randomTrait.interpret("k", tense);
                return ret.Replace("  ", " ");
            }
            randomTrait = (Trait)traitList[r.Next(traitList.Count)];
            if (code.Equals("ID"))
            {
                return randomTrait.getId();
            }
            //if (code.IndexOf('C') > -1)
            //{
            //    adj = randomTrait.Colour;
            //}

            // ensure as much as possible that all the requirements of the user is met. 
            for (int j = 0; j < 100; j++)
            {
                randomTrait = (Trait)traitList[r.Next(traitList.Count)];
                if (code.IndexOf('A') > -1 && randomTrait.Adjectives.Count == 0) continue;
                if (code.IndexOf('S') > -1 && randomTrait.States.Count == 0) continue;
                //if (code.IndexOf('K') > -1 && randomTrait.Aliases.Count == 0) continue;
                break;
            }
            // ensure at least a moniker.
            //if (code.IndexOf('K') > -1 && randomTrait.Aliases.Count == 0) return "";


            if (code.IndexOf('A') > -1)
            {
                for (int i = 0; i < 100; i++)
                {
                    if (randomTrait.Adjectives is ListDrawString)
                    {
                        randomAdjective = (randomTrait.Adjectives as ListDrawString).draw();
                        if (!String.IsNullOrWhiteSpace(randomAdjective))
                        {
                            break;
                        }
                    }
                }
                adj += " " + randomAdjective;
            }
            if (code.IndexOf('S') > -1)
            {
                for (int i = 0; i < 100; i++)
                {
                    if (randomTrait.States is ListDrawString)
                    {
                        randomAdjective = (randomTrait.States as ListDrawString).draw();
                        if (!String.IsNullOrWhiteSpace(randomAdjective))
                        {
                            break;
                        }
                    }
                }
                adj += " " + randomAdjective;
            }
            if (code.IndexOf('K') > -1)
            {
                adj += " " + randomTrait.interpret("k", tense);
            }

            return adj.Trim();
        }

        // returns -1 if no code is found or misformed.
        private int getGarmentIdFromScriptCode(string code) {

            Regex reg = new Regex(@"\d$");

            string gid = "";
            int groupId = -1;

            // Regex = "^G(ID|C|A|F){1,3}\d*$"

            // If the code is a digit, iterate all the matches and concat it to the gid string.
            // it ignores all the string characters and concats numbers regardless of where they are.
            // However, the regex that allowed the code to reach this point is formed as above and prevents funky syntax. Ex GID22
            while (reg.IsMatch(code))
            {
                gid = code.Substring(code.Length - 1, 1) + gid;
                code = code.Remove(code.Length - 1, 1);

            }


            // If the GID (a string of an integer digit) is not empty...
            if (!String.IsNullOrWhiteSpace(gid)) groupId = int.Parse(gid);

            return groupId;
        }


        protected List<T> collectListFromGroupId<T>(int groupId, List<T> biggerList) where T : Matter
        {
            List<T> list = new List<T>();
            foreach (T item in biggerList)
            {
                // Item must be marked as usable for random.
                if (item.IsUsed)
                {
                    // Secret non-implemented bypass? ;; -1 Not yet accessible via Gui.
                    if (groupId == -1)
                        list.Add(item);
                    else
                        // Add all garments, whose groupId matches the one in the first parameter passed to this function, to a list.  
                        if (item.GroupId == groupId) list.Add(item);
                }
            }


            return list;
        }


        private Garment lastRandomGarment = null;
        private Garment getLastRandomlySelectedGarment()
        {
            return this.lastRandomGarment;
        }
        private string getRandomGarment(string code, int groupId = 0)
        {
            if (this.garments.Count == 0) return "";
            //Garment g = (Garment)this.garments[r.Next(this.garments.Count)];

            List<Garment> selectedGarments = new List<Garment>();
            //Regex reg = new Regex(@"\d$");

            string gid = "";

            groupId = getGarmentIdFromScriptCode(code);



            // Regex = "^G(ID|C|A|F){1,3}\d*$"

            // If the code is a digit, iterate all the matches
            //while (reg.IsMatch(code))
            //{
            //gid = code.Substring(code.Length - 1, 1) + gid;
            //code = code.Remove(code.Length - 1, 1);

            //}


            // If the GID (a string of an integer digit) is not empty...
            //if(!String.IsNullOrWhiteSpace(gid)) groupId = int.Parse(gid);



            // In a list, collect all the garments with the groupId
            foreach (Garment garment in this.garments)
            {
                if (garment.IsUsed)
                {
                    // Secret non-implemented bypass? ;; -1 Not yet accessible via Gui.
                    if (groupId == -1)
                        selectedGarments.Add(garment);
                    else
                        // Add all garments, whose groupId matches the one in the first parameter passed to this function, to a list.  
                        if (garment.GroupId == groupId) selectedGarments.Add(garment);
                }
            }


            // if the list is empty...
            if (selectedGarments.Count == 0) return "";


            // Get a random garment
            Garment g = (Garment)selectedGarments[r.Next(selectedGarments.Count)];
            string adj = "";

            if (g == null)
            {
                // if g is null, something serious happened.
                return null;
            }

            this.lastRandomGarment = g;

            // if the code is 'ID', return the garment's id
            if (code.Equals("ID"))
            {
                return g.getId();
            }


            // If the code has 'A', return an adjective associated to the garment.
            if (code.IndexOf('A') > -1)
            {
                if (g.Adjectives.Count > 0)
                {
                    if (g.Adjectives is ListDrawString)
                    {
                        adj += " " + (g.Adjectives as ListDrawString).draw();
                    }
                    else
                        adj += " " + g.Adjectives[r.Next(g.Adjectives.Count)];

                }
            }

            // If the code has a 'C', concat an ajective to the ajective string. 
            if (code.IndexOf('C') > -1) {
                adj += " " + g.Colour;
            }


            // If the code has an 'F', concat the fabric of the garment to the ajective string. 
            if (code.IndexOf('F') > -1)
            {
                adj += " " + g.FabricType;
            }


            // Return the values. 
            return (adj + " " + g.Name).Trim();
        }

        public override Dictionary<string, string> help()
        {
            Dictionary<string, string> d = base.help();
            d.Add("FN", "Gets the first name.");
            d.Add("MN", "Gets the middle name.");
            d.Add("LN", "Gets the family name.");
            d.Add("G", "Gets a random garment.");
            d.Add("GC", "Gets a garment with the colour.");
            d.Add("GF", "Gets a garment with the fabric type.");
            d.Add("GA", "Gets a garment with an adjective.");
            d.Add("GCA", "Gets a garment with the colour and adjective.");
            d.Add("GCF", "Gets a garment with the colour and fabric.");
            d.Add("GCFA", "Gets a garment with the colour, fabric, and adjective. G must come first, but order is not important after that.");
            d.Add("PP", "Gets a personal possesive pronoun.");
            d.Add("SP", "Gets a subject pronoun.");
            d.Add("OP", "Gets an object pronoun.");
            d.Add("PA", "Gets a possessive adjective pronoun.");
            d.Add("RP", "Gets a reflexive pronoun.");
            d.Add("T", "Gets a random trait.");
            d.Add("T#", "Gets a random trait from a specific groudId number identifier. GroupId must follow the T.");
            d.Add("TC", "Gets a random trait with its colour.");
            d.Add("TA", "Gets a random trait with a random adjective.");
            d.Add("TK", "Gets a random trait's alias");
            d.Add("T#M", "Gets a random trait starting with possessive pronoun.");
            d.Add("TCAK", "Gets a random trait's alias with colour and random adjective.");
            d.Add("T2CA", "Gets a random trait's alias in groupId 2 with colour and random adjective and alias. GroupId must follow the T.");
            d.Add("CLOTH", "Gets the last garment used, and does a '$' code from it.");
            d.Add("MYCLOTH", "Gets the YSR code from the last garment selected.");



            return d;
        }

        #endregion

        public override DBbase getDBObejct(DBbase d)
        {

            DBHuman h = d as DBHuman;
            if (h == null) h = new DBHuman();

            h.familyName = this.familyName;
            h.firstName = this.firstName;
            h.middleName = this.middleName;
            h.age = this.Age;
            h.hasVagina = this.HasVagina ? 1 : 0;
            h.height = this.Height;
            h.height = this.Height;
            h.species = this.Spieces;
            h.isAnIt = this.IsAnIt ? 1 : 0;
            h.isMany = this.IsMany ? 1 : 0;
            h.is_pov = this.isPov ? 1 : 0;
            h.perspectiveId = this.perspective;
            h.scripts = String.Join("|", this.scripts.getExpandedList().ToArray());

            h.charactersId = this.charactersId;
            //if (this.getUserModifiedCharacter() != null)
            //{
            //    h.usercharactersId = this.getUserModifiedCharacter().id;
            //}

            h.hasVagina_readonly = this.hasVagina_readonly ? 1 : 0;
            h.familyName_readonly = this.familyName_readonly ? 1 : 0;
            h.firstName_readonly = this.firstName_readonly ? 1 : 0;
            h.isAnIt_readonly = this.isAnIt_readonly ? 1 : 0;
            h.isMany_readonly = this.isMany_readonly ? 1 : 0;
            h.middleName_readonly = this.middleName_readonly ? 1 : 0;
            h.perspective_readonly = this.perspective_readonly ? 1 : 0;
            h.scripts_readonly = this.scripts_readonly ? 1 : 0;

            return base.getDBObejct(h);
        }

        public override void setFromDBObject(DBbase o)
        {
            base.setFromDBObject(o);
            DBHuman h = o as DBHuman;
            this.Age = h.age;
            this.firstName = h.firstName;
            this.middleName = h.middleName;
            this.familyName = h.familyName;
            this.HasVagina = h.hasVagina == 1 ? true : false;
            this.Height = h.height;
            this.Weight = h.weight;
            this.Spieces = h.species;
            this.IsMany = h.isMany == 1 ? true : false;
            this.IsAnIt = h.isAnIt == 1 ? true : false;
            this.isPov = h.is_pov == 1 ? true : false;
            this.perspective = h.perspectiveId;
            this.charactersId = h.charactersId;
            //if (this.getUserModifiedCharacter() != null)
            //{
            //    this.getUserModifiedCharacter().charactersId = h.usercharactersId;
            //}

            this.hasVagina_readonly = h.hasVagina_readonly == 1 ? true : false;
            this.familyName_readonly = h.familyName_readonly == 1 ? true : false;
            this.firstName_readonly = h.firstName_readonly == 1 ? true : false;
            this.isAnIt_readonly = h.isAnIt_readonly == 1 ? true : false;
            this.isMany_readonly = h.isMany_readonly == 1 ? true : false;
            this.middleName_readonly = h.middleName_readonly == 1 ? true : false;
            this.perspective_readonly = h.perspective_readonly == 1 ? true : false;
            this.scripts_readonly = h.scripts_readonly == 1 ? true : false;

            ListDrawString.SetStringListFromCSV(this.scripts, h.scripts);
        }

        public override bool Equals(System.Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            Human p = obj as Human;
            if ((System.Object)p == null)
            {
                return false;
            }

            // check for reference equality.
            if (System.Object.ReferenceEquals(this, p)) return true;

            // Return true if the fields match:

            return
                //(this.isAnIt == p.isAnIt) &&
                //(this.IsMany == p.IsMany) &&
                //(this.middleName.Equals(p.middleName)) &&
                //(this.familyName.Equals(p.familyName)) &&
                //(this.firstName.Equals(p.firstName)) &&
                (this.getFullId().Equals(p.getFullId())) // &&
                                                         //(this.hasVagina == p.hasVagina) &&
                                                         //(this.perspective == p.perspective) &&
                                                         //(this.Age == p.Age)
                ;
        }

        private enum GarmentTypes
        {
            top_under,
            top_over,
            bottom_under,
            bottom_over,
            socks,
            shoes,
            bra,
            jacket,
            scarf,
            hat,
            eyewear,
            necklace,
            bracelet,
            ankle_bracelet,
            waist_ring,
            finger_ring,
            belt,
            ear_ring,
            purse,
            backpack


        }

        private class Connectors
        {
            public string Name = "";
            public string MaleTrait = "";
            public string FemaleTrait = "";

            public Connectors()
            {

            }

            public Connectors(string name, string maleTrait, string femaleTrait)
            {
                this.Name = name;
                this.MaleTrait = maleTrait;
                this.FemaleTrait = femaleTrait;
            }
        }

        public static Human factory(bool setDefaults, int globalPercentage, bool isFemale = true)
        {
            Human person = new Human();
            if (setDefaults) { 
                person.FirstName = Human.defaultFirstName;
                person.MiddleName = Human.defaultMiddleName;
                person.FamilyName = Human.defaultLastName;
                person.GlobalPercentage = globalPercentage;
                person.Id = Human.defaultScriptId;

                person.AddAdjective("[this.c]");
                person.AddAdjective("[this.z]");
            }
            //person.AddAlias("[this.fn]");

            string[] goodTraits = { "body", "tits", "butt", "cleavage", "curves", "eyes", "face","figure","hair","hips","legs", "lips","skin","thighs","waist"  };
            string[] traits = { "abs", "arm", "bulge", "arms", "ass cheeks", "back", "asshole", "back",   "belly button", "feet",  "finger", "fingernails", "fingers", "fingertips",  "hand", "hands", "head", "hip",  "juices", "knees",  "mouth", "nipples", "palm", "pussy",  "pussy lips",  "stomach", "sweat",  "throat", "toes", "tongue", "voice",  "pectorials", "biceps", "triceps", "thigh gap", "bikini bridge", "penis", "sweat", "throat", "bulge", "nose" };
            string[] garments = { "footwear", "gloves", "scarf", "hat", "glasses", "outfit", "suit", "pants", "shirt", "bra", "corset", "earrings", "ring", "ankle bracelet", "bracelet", "underwear", "socks", "stockings", "necklace", "waist clincher", "dildo" };

            //string[] connector = { "crotch", "crotch_arousal", "masturbation", "crotch_orgasm", "breast_caress", "breast_size", "waist_size", "hip_size" };

            string[] traitsThatArePlural = { "tits", "curves", "eyes", "hips", "legs", "lips", "thighs", "abs", "arms", "feet", "ass cheeks", "fingernails", "fingers", "fingertips", "hands", "juices", "knees", "nipples", "pussy lips", "toes", "pectorials", "biceps", "triceps" };
            string[] garmentsThatArePlural = { "gloves", "footwear", "gloves", "glasses", "pants", "earrings", "socks", "stockings" };


            Dictionary<string, GarmentTypes> garmentTypes = new Dictionary<string, GarmentTypes>();

            List<Connectors> connectors = new List<Connectors>();
            connectors.Add(new Connectors("crotch", "penis", "pussy"));
            connectors.Add(new Connectors("flamingo", "penis", "tits"));
            connectors.Add(new Connectors("waistsize", "biceps", "waist"));
            connectors.Add(new Connectors("hipsize", "shoulders", "hips"));
            connectors.Add(new Connectors("fuckhole", "asshole", "pussy"));
            connectors.Add(new Connectors("penetrator", "penis", "dildo"));
            

            foreach(string trait in traits)
            {
                string formattedString = trait.Trim().Replace(" ", "_");
                factoryAddTrait(person, formattedString, 1);
            }
            foreach (var c in connectors)
            {
                string formattedString = c.Name.Trim().Replace(" ", "_");
                //factoryAddTrait(person, formattedString, 47);
                person.addTrait(Trait.createConjunction(formattedString, "gender==male", c.MaleTrait, c.FemaleTrait));
            }

            foreach (string trait in goodTraits)
            {
                string formattedString = trait.Trim().Replace(" ", "_");
                factoryAddTrait(person, formattedString, 0);
            }

            foreach (string garment in garments)
            {
                string formattedString = garment.Trim().Replace(" ", "_");
                factoryAddGarment(person, formattedString);
            }

            foreach (string str in traitsThatArePlural)
            {
                foreach(Trait t in person.getTraitsAsTraitsList())
                {
                    if (t.Id.Equals(str))
                    {
                        t.IsMany = true;
                        t.setPronoun(Pronouns.Pronoun.They);
                    }
                }
            }

            //foreach (var connectingTrait in connectors)
            //{
            //    foreach (Trait t in person.getTraitsAsTraitsList())
            //    {
            //        if (t.Id.Equals(connectingTrait.Name))
            //        {
            //            t.AddAjective("[owner.gender==male?[owner." + connectingTrait.MaleTrait + ".A-A]:[owner." + connectingTrait.FemaleTrait + ".A-A]]");
            //            t.addAdverb("[owner.gender==male?[owner." + connectingTrait.MaleTrait + ".V-A]:[owner." + connectingTrait.FemaleTrait + ".V-A]]");
            //            t.AddAlias("[owner.gender==male?[owner." + connectingTrait.MaleTrait + ".K-A]:[owner." + connectingTrait.FemaleTrait + ".K-A]]");
            //            t.addState("[owner.gender==male?[owner." + connectingTrait.MaleTrait + ".S-A]:[owner." + connectingTrait.FemaleTrait + ".S-A]]");
            //            t.addWho("[owner.gender==male?[owner." + connectingTrait.MaleTrait + ".F-A]:[owner." + connectingTrait.FemaleTrait + ".F-A]]");
            //        }
            //    }
            //}

            foreach (string str in garmentsThatArePlural)
            {
                foreach (Garment g in person.getGarmentsAsGarmentList())
                {
                    if (g.Id.Equals(str)) 
                    {
                        g.IsMany = true;
                        g.setPronoun(Pronouns.Pronoun.They);
                    }
                }
            }

            Human.addCouldTrait(person);

            return person;
        }

        private static void addCouldTrait(Human person)
        {
            Trait could = new Trait("could");
            could.AddAlias("[owner.v(have)] the ability to");
            could.AddAlias("[owner.v(be)] able to");
            could.AddAlias("[owner.v(be)] capable to");
            could.AddAlias("[owner.v(be)] equal to the task of");
            could.AddAlias("[owner.v(have)] what it [owner.asit.v(take)] to");
            could.AddAlias("[owner.v(have)] the power to");
            could.AddAlias("[owner.v(have)] the means to");
            could.AddAlias("[owner.v(have)] the opportunity to");
            could.AddAlias("[owner.v(have)] the competency to");
            could.AddAlias("[owner.v(have)] the capability to");
            could.AddAlias("[owner.v(have)] the chance to");
            could.AddAlias("[owner.v(have)] the option to");
            could.AddAlias("[owner.v(have)] the possibility to");
            //could.AddAlias("");

            could.addAdverb("[owner.v(-have)] the ability to");
            could.addAdverb("[owner.v(-be)] able to");
            could.addAdverb("[owner.v(-be)] capable to");
            could.addAdverb("[owner.v(-be)] equal to the task of");
            could.addAdverb("[owner.v(-have)] what it [owner.asit.v(-take)] to");
            could.addAdverb("[owner.v(-have)] the power to");
            could.addAdverb("[owner.v(-have)] the means to");
            could.addAdverb("[owner.v(-have)] the competency to");
            could.addAdverb("[owner.v(-have)] the opportunity to");
            could.addAdverb("[owner.v(-have)] the capability to");
            could.addAdverb("[owner.v(-have)] the possibility to");
            could.addAdverb("[owner.v(-have)] the likeliness to");
            could.addAdverb("[owner.v(-have)] the competency to");
            //could.addAdverb("");

            could.GroupId = 53;

            person.addTrait(could);
        }

        private static void factoryAddTrait(Human person, string traitName, int groupNumber)
        {
            Trait trait = new Trait(traitName);
            trait.Id = traitName;
            trait.GroupId = groupNumber;
            //trait.Size = "";
            //trait.addState("[this.c]");
            //trait.addState("[this.z]");
            trait.AddAlias("[this.n]");


            person.addTrait(trait);
        }

        private static void factoryAddGarment(Human person, string garmentName)
        {
            Garment garment = new Garment(garmentName);
            garment.Id = garmentName;
            garment.addState("[this.c]");
            garment.addState("[this.z]");
            garment.Location = "on [this.body.my.yar]";
            garment.Size = "tight";
            garment.Colour = "white";
            garment.AddAlias("[this.n]");
            garment.addWho("midClause => , which [this.v(be).~ located.l],");
            garment.addWho("startClause => Located [this.l],");
            garment.addGarmentDescription("[owner.sp.v(wear)] [this.art.yar]");
            garment.addGarmentDescription("[owner.sp.v(wear)] [this.art.yar]");
            garment.addGarmentDescription("[owner.sp.v(wear)] [this.yar]");
            garment.addGarmentDescription("[owner.sp.v(wear)] [this.yar]");
            person.addGarment(garment);
        }

        public void merge(Human other)
        {
            merge(this, other);

            //if (other == null) return;
            //foreach (var item in other.Adjectives) this.AddAdjective(item);
            //foreach (var item in other.Adverbs) this.addAdverb(item);
            //foreach (var item in other.Aliases) this.AddAlias(item);
            //foreach (var item in other.Scripts) this.addScripts(item);
            //foreach (var item in other.States) this.addState(item);
            //foreach (var item in other.Who) this.addWho(item);

            //var list = this.getTraitsAsTraitsList();
            //foreach (var trait in other.getTraitsAsTraitsList())
            //{
            //    if (list.Contains(trait))
            //    {
            //        Trait t = list[list.IndexOf(trait)];
            //        t.combine(trait);
            //    }
            //    else
            //    {
            //        list.Add(trait);
            //    }
            //}

            //var garmentList = this.getGarmentsAsGarmentList();
            //foreach (var garment in other.getGarmentsAsGarmentList())
            //{
                
            //    if (garmentList.Contains(garment))
            //    {
            //        Garment g = garmentList[garmentList.IndexOf(garment)];
            //        g.combine(garment);
            //    }
            //    else
            //    {
            //        garmentList.Add(garment);
            //    }
            //}
            
        }

        public enum Anonymous
        {
            AsItMany,
            AsIt,
            As3rd,
            As3rdMany,
            As3rdMale,
            As3rdFemale

        }

        public static Human factory(Anonymous anonymous)
        {
            Human third = new Human();

            // set the script id multiAdjectiveNounPhrase(code, "$", "the");
            Character c = new Character(-1, anonymous.ToString().ToLower());
            c.IsImportant = false;
            third.Character = c;
            //UserCharacter uc = new UserCharacter(-1);
            //third.setUserModifiedCharacter(uc);
            anonymousUniversalSet(third);
            anonymousSetParticulars(third, anonymous);
            Human.addCouldTrait(third);

            return third;
        }

        private static void anonymousSetParticulars(Human third, Anonymous anonymous)
        {
            third.IsMany = false;
            third.IsAnIt = false;
            third.HasVagina = false;
            third.setPronoun(Pronouns.Pronoun.It);

            switch (anonymous)
            {
                case Anonymous.As3rdFemale:
                    third.HasVagina = true;
                    third.AddAlias("woman");
                    third.setPronoun(Pronouns.Pronoun.Female);
                    break;
                case Anonymous.As3rdMale:
                    third.AddAlias("man");
                    third.setPronoun(Pronouns.Pronoun.Male);
                    break;
                case Anonymous.As3rdMany:
                    third.IsMany = true;
                    third.setPronoun(Pronouns.Pronoun.They);
                    break;
                case Anonymous.AsIt:
                    third.IsAnIt = true;
                    break;
                case Anonymous.AsItMany:
                    third.IsAnIt = true;
                    third.IsMany = true;
                    third.setPronoun(Pronouns.Pronoun.They);
                    break;
                case Anonymous.As3rd:
                    third.IsAnIt = true; // to get the pronouns.
                    third.setPronoun(Pronouns.Pronoun.SingularThey);
                    break;
            }
        }

        private static void anonymousUniversalSet(Human third)
        {
            third.Perspective = 3;
            third.AddAlias("someone");
            third.AddAlias("somebody");
            third.AddAlias("person");
            third.FirstName = "someone";
        }

        // remove all "universal" traits that are hardcoded, and referencial ones.
        public void removeUniveralTraits()
        {

            this.removeTrait("conditionals");
            this.removeTrait("could");


            // remove references
            var list = getTraitsThatAreReferencial();

            foreach (var item in list)
            {
                this.removeTrait(item);
            }

        }

        private List<Trait> getTraitsThatAreReferencial()
        {
            List<Trait> list = new List<Trait>();
            foreach (var item in this.getTraitsAsTraitsList())
            {

                if (item.getId().ToLower().StartsWith("ref"))
                {
                    list.Add(item);
                }
            }

            return list;
        }

        public List<string> getReversePronounList()
        {
            List<string> list = new List<string>();
            int adjectiveCount = this.getAdjectives().Length;
            int aliasCount = this.getAliases().Length;
            int stateCount = this.getStates().Length;


            list.Add(this.getFullId() + ".FN.LN");

            // if there are aliases
            if (aliasCount > 0)
            {
                if (adjectiveCount > 0)
                {
                    if (adjectiveCount > 2) list.Add(this.getFullId() + ".A3.K");
                    if (adjectiveCount > 1) list.Add(this.getFullId() + ".A2.K");
                    list.Add(this.getFullId() + ".$.F");
                    list.Add(this.getFullId() + ".@.F");
                    list.Add(this.getFullId() + ".$");
                    list.Add(this.getFullId() + ".@");
                    list.Add(this.getFullId() + ".$$");
                    list.Add(this.getFullId() + ".@@");
                    list.Add(this.getFullId() + ".SA.K");
                }

                if (stateCount > 0)
                {
                    if (stateCount > 2) list.Add(this.getFullId() + ".S3.K");
                    if (stateCount > 1) list.Add(this.getFullId() + ".S2.K");
                    if(adjectiveCount > 0) list.Add(this.getFullId() + ".AS.K");

                }

                list.Add(this.getFullId() + ".K");

                if (adjectiveCount > 0)
                {
                    list.Add(this.getFullId() + ".++");
                    list.Add(this.getFullId() + ".+");
                    list.Add(this.getFullId() + ".%");
                    list.Add(this.getFullId() + ".%%");
                }
            }

            
            list.Add(this.getFullId() + ".SPP");
            list.Add(this.getFullId() + ".N");
            
            
            
            

            // Remove the unnessesary traits.
            this.removeUniveralTraits();

            foreach (Trait t in this.getTraitsAsTraitsList())
            {
                adjectiveCount = t.getAdjectives().Length;
                aliasCount = t.getAliases().Length;
                stateCount = t.getStates().Length;

                if (adjectiveCount > 0)
                {
                    list.Add(t.getFullId() + ".my.A3.K");
                    list.Add(t.getFullId() + ".my.A2.K");
                    list.Add(t.getFullId() + ".my.YAR");
                    list.Add(t.getFullId() + ".$");
                    list.Add(t.getFullId() + ".@");
                    list.Add(t.getFullId() + ".$$");
                    list.Add(t.getFullId() + ".@@");
                    
                }

                if (stateCount > 0)
                {
                    list.Add(t.getFullId() + ".my.S3.K");
                    list.Add(t.getFullId() + ".my.S2.K");
                    list.Add(t.getFullId() + ".my.YSR");
                    list.Add(t.getFullId() + ".my.SA.K");
                    if (adjectiveCount > 0) list.Add(t.getFullId() + ".my.AS.K");
                }

                if (adjectiveCount > 0)
                {
                    list.Add(t.getFullId() + ".++");
                    list.Add(t.getFullId() + ".+");
                    list.Add(t.getFullId() + ".%");
                    list.Add(t.getFullId() + ".%%");
                }
                
                list.Add(t.getFullId() + ".my.N");
                list.Add(t.getFullId() + ".my.K");
                
            }

            foreach (Garment t in this.getGarmentsAsGarmentList())
            {
                adjectiveCount = t.getAdjectives().Length;
                aliasCount = t.getAliases().Length;
                stateCount = t.getStates().Length;
                int garmentCount = t.getGarmentsArray().Length;

                if (adjectiveCount > 0)
                {
                    if (garmentCount > 0)
                    {
                        list.Add(t.getFullId() + ".my.A3.K.GT");
                    }

                    


                    list.Add(t.getFullId() + ".my.A3.K");
                    list.Add(t.getFullId() + ".my.A2.K");
                    list.Add(t.getFullId() + ".my.YAR");
                    list.Add(t.getFullId() + ".$");
                    list.Add(t.getFullId() + ".@");
                    list.Add(t.getFullId() + ".$$");
                    list.Add(t.getFullId() + ".@@");

                }

                if (stateCount > 0)
                {
                    list.Add(t.getFullId() + ".my.S3.K");
                    list.Add(t.getFullId() + ".my.S2.K");
                    list.Add(t.getFullId() + ".my.YSR");
                    list.Add(t.getFullId() + ".my.SA.K");
                    if (adjectiveCount > 0) list.Add(t.getFullId() + ".my.AS.K");
                }

                if (adjectiveCount > 0)
                {
                    list.Add(t.getFullId() + ".++");
                    list.Add(t.getFullId() + ".+");
                    list.Add(t.getFullId() + ".%");
                    list.Add(t.getFullId() + ".%%");
                }

                list.Add(t.getFullId() + ".my.N");
                list.Add(t.getFullId() + ".my.K");

                
            }

            return list;
        }


        public void merge(Human humanToMergeTo, Human humanToMerge)
        {
            if (humanToMergeTo == null || humanToMerge == null) return;
            base.merge(humanToMergeTo, humanToMerge);

            // do it by reference
            //merge the lists
            humanToMergeTo.addScripts(humanToMerge.Scripts.ToArray());

            foreach (Garment g in humanToMerge.garments)
            {
                humanToMergeTo.mergeGarment(g.getClone() as Garment);
            }

            foreach (Trait t in humanToMerge.traits)
            {
                humanToMergeTo.mergeTrait(t.getClone() as Trait);
            }

        }

        //else if (cmd.ToLower().Equals("asmany"))
        //{
        //    if (actor is Human)
        //    {
        //        Human thisHuman = (actor as Human).getClone() as Human;


        //        if (thisHuman.IsAnIt)
        //        {
        //            thisHuman.IsMany = !thisHuman.IsMany;
        //            actor = thisHuman;
        //        }
        //        else
        //        {
        //            Human otherHuman = null;
        //            if (String.IsNullOrWhiteSpace(thisHuman.Other)) return new KeyValuePair<ActionParser, Queue<string>>(actor, queue);
        //            ActionParser ap = (ActionParser)this.storyMoment.find(thisHuman.Other);
        //            if (ap is Human)
        //            {
        //                otherHuman = ap as Human;
        //            }
        //            else
        //                return new KeyValuePair<ActionParser, Queue<string>>(actor, queue);


        //            int perspective = (otherHuman.Perspective * 3) + (thisHuman.Perspective * 3);
        //            Human many = (actor as Human).getClone() as Human;
        //            many.IsMany = true;
        //            if (perspective == 18)
        //                many.Perspective = 3;
        //            else if (perspective < 13)
        //                many.Perspective = 1;
        //            else
        //                many.Perspective = 2;


        //            actor = many;
        //        }
        //    }
        //}
        //else if (cmd.ToLower().Equals("it") || cmd.ToLower().Equals("asit") || cmd.ToLower().Equals("asanit") || cmd.ToLower().Equals("asthing"))
        //{
        //    if (actor is Human)
        //    {
        //        Human thisHuman = (actor as Human);
        //        if (!thisHuman.IsAnIt)
        //        {
        //            Human it = new Human();
        //            it.Character = thisHuman.Character;
        //            it.IsMany = false;
        //            it.IsAnIt = true;
        //            it.Perspective = 3;
        //            actor = it;
        //        }
        //    }

        //}
        //else if (cmd.ToLower().Equals("as3rd") || cmd.ToLower().Equals("3rd"))
        //{
        //    if (actor is Human)
        //    {
        //        Human third = new Human();
        //        third.Character = (actor as Human).Character;
        //        third.IsMany = true;
        //        third.IsAnIt = false;
        //        third.HasVagina = false;
        //        third.Perspective = 3;
        //        third.AddAlias("someone");
        //        third.AddAlias("somebody");
        //        third.AddAlias("person");
        //        third.FirstName = "someone";
        //        actor = third;
        //    }

        //}
        //else if (cmd.ToLower().Equals("as3rdm") || cmd.ToLower().Equals("3rdm"))
        //{
        //    if (actor is Human)
        //    {
        //        Human third = new Human();
        //        third.Character = (actor as Human).Character;
        //        third.IsMany = false;
        //        third.IsAnIt = false;
        //        third.HasVagina = false;
        //        third.Perspective = 3;
        //        third.AddAlias("someone");
        //        third.AddAlias("somebody");
        //        third.AddAlias("person");
        //        third.AddAlias("man");
        //        third.FirstName = "someone";
        //        actor = third;
        //    }

        //}
        //else if (cmd.ToLower().Equals("as3rdf") || cmd.ToLower().Equals("3rdf"))
        //{
        //    if (actor is Human)
        //    {
        //        Human third = new Human();
        //        third.Character = (actor as Human).Character;
        //        third.IsMany = false;
        //        third.IsAnIt = false;
        //        third.HasVagina = true;
        //        third.Perspective = 3;
        //        third.AddAlias("someone");
        //        third.AddAlias("somebody");
        //        third.AddAlias("person");
        //        third.AddAlias("woman");
        //        third.FirstName = "someone";
        //        actor = third;
        //    }

        //}

    }
}

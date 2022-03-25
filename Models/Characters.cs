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
    public class Character : JSerializable<Character>, IComparable, DBActionable
    {
        public int Id { get; set; } = 0;
        public int OwnerId { get; set; } = 0;
        public int Age { get; set; } = 0;
        public int Weight { get; set; } = 0;
        public string Height { get; set; } = "";
        public string Colour { get; set; } = "";
        public string Size { get; set; } = "";
        public bool HasVagina { get; set; } = true;
        public bool IsImportant { get; set; } = true;
        public bool IsAnIt { get; set; } = false;
        public bool IsMany { get; set; } = false;
        public int Perspective { get; set; } = 3;
        public string Pronouns { get; set; } = Classes.Pronouns.getStringFromPronoun(Classes.Pronouns.Pronoun.Female);

        public int LinkedCharacterId { get; set; } = 0;
        public string LinkedCharacterName { get; set; } = "";
        public string _scriptId = "";
        public string _firstName = "";
        public string _middleName = "";
        public string _familyName = "";
        public string _descrption = "";

        public string ScriptId
        {
            get
            {
                if (String.IsNullOrWhiteSpace(this._scriptId)) return "";
                if (this._scriptId == Human.defaultScriptId) return "";
                return this._scriptId;
            }
            set
            {
                this._scriptId = value;
            }
        }
        public string FirstName 
        {
            get
            {
                if (String.IsNullOrWhiteSpace(this._firstName)) return "";
                if (this._firstName == Human.defaultFirstName) return "";
                return this._firstName;
            }
            set
            {
                this._firstName = value;
            }
        }
        public string MiddleName
        {
            get
            {
                if (String.IsNullOrWhiteSpace(this._middleName)) return "";
                if (this._middleName == Human.defaultMiddleName) return "";
                return this._middleName;
            }
            set
            {
                this._middleName = value;
            }
        }
        public string FamilyName
        {
            get
            {
                if (String.IsNullOrWhiteSpace(this._familyName)) return "";
                if (this._familyName == Human.defaultLastName) return "";
                return this._familyName;
            }
            set
            {
                this._familyName = value;
            }
        }
        public string Descrption
        {
            get
            {
                if (String.IsNullOrWhiteSpace(this._descrption)) return "";
                return this._descrption;
            }
            set
            {
                this._descrption = value;
            }
        }
        

        public Character()
        {
            Id = -1;
            OwnerId = -1;
            Age = 18;
            ScriptId = "";
            FirstName = "";
            MiddleName = "";
            FamilyName = "";
            HasVagina = true;
            Descrption = "";
            this.LinkedCharacterId = 0;
            IsImportant = true;
        }

        public Character(int userId) : this()
        {
            this.OwnerId = userId;
        }

        public Character(int userId, string scriptId)
            : this(userId)
        {
            this.ScriptId = scriptId;
        }

        public Character(int userId, string scriptId, string firstName, string middleName, string familyName, bool hasVagina)
            : this(userId, scriptId)
        {
            this.FirstName = firstName;
            this.FamilyName = familyName;
            this.MiddleName = middleName;
            this.HasVagina = hasVagina;
        }

        public Character(int userId, string scriptId, string firstName, string middleName, string familyName, bool hasVagina, string description)
            : this(userId, scriptId, firstName, middleName, familyName, hasVagina)
        {
            this.Descrption = description;
        }

        public Character(Human human)
        {
            set(human);
        }

        public void set(Character character)
        {
            this.OwnerId = character.OwnerId;
            this.ScriptId = character.ScriptId;
            this.FirstName = character.FirstName;
            this.MiddleName = character.MiddleName;
            this.FamilyName = character.FamilyName;
            this.HasVagina = character.HasVagina;
            this.Colour = character.Colour;
            this.Age = character.Age;
            this.Height = character.Height;
            this.IsAnIt = character.IsAnIt;
            this.IsMany = character.IsMany;
            this.IsImportant = true;
            this.Weight = character.Weight;
            this.Size = character.Size;
            this.Perspective = character.Perspective;
            this.Pronouns = character.Pronouns;
            this.LinkedCharacterName = character.LinkedCharacterName;
            this.LinkedCharacterId = character.LinkedCharacterId; 

        }

        public void set(Human human)
        {
            this.OwnerId = human.userId;
            this.ScriptId = human.getId();
            this.FirstName = human.FirstName;
            this.MiddleName = human.MiddleName;
            this.FamilyName = human.FamilyName;
            this.HasVagina = human.HasVagina;
            this.Colour = human.Colour;
            this.Age = human.Age;
            this.Height = human.Height;
            this.IsAnIt = human.IsAnIt;
            this.IsMany = human.IsMany;
            this.IsImportant = true;
            this.Weight = human.Weight;
            this.Size = human.Size;
            this.Perspective = human.Perspective;
            this.Pronouns = Classes.Pronouns.getStringFromPronoun(human.pronouns);
        }
        public static T ObjectFromJson<T>(string json) where T : Character
        {
            try
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception)
            {

                return null;
            }
        }

        public string getJson()
        {
            string muhString = Newtonsoft.Json.JsonConvert.SerializeObject(this);
            return muhString;
        }

        public Character getClone()
        {
            DBCharacters dbMatter = new DBCharacters();
            dbMatter = this.getDBObejct(dbMatter) as DBCharacters;
            Character h = new Character();
            h.setFromDBObject(dbMatter);

            return h;
        }

        //public Character(SerializationInfo info, StreamingContext ctxt)
        //{
        //    if(this.LinkedCharacterName.Equals(this.ScriptId))
        //    {
        //        this.LinkedCharacterName = "";
        //        this.LinkedCharacterId = 0;
        //    }
        //    try
        //    {
        //        this.FirstName = (string)info.GetValue("firstName", typeof(string));
        //    }
        //    catch (Exception)
        //    { }
        //    try
        //    {
        //        this.MiddleName = (string)info.GetValue("middleName", typeof(string));
        //    }
        //    catch (Exception)
        //    { }
        //    try
        //    {
        //        this.FamilyName = (string)info.GetValue("familyName", typeof(string));
        //    }
        //    catch (Exception)
        //    { }
        //    try
        //    {
        //        this.HasVagina = (bool)info.GetValue("hasVagina", typeof(bool));
        //    }
        //    catch (Exception)
        //    { }
        //    try
        //    {
        //        this.ScriptId = (string)info.GetValue("scriptId", typeof(string));
        //    }
        //    catch (Exception)
        //    { }
        //    try
        //    {
        //        this.Age = (int)info.GetValue("age", typeof(int));
        //    }
        //    catch (Exception)
        //    { }
        //    try
        //    {
        //        this.IsImportant = (bool)info.GetValue("is_important", typeof(bool));
        //    }
        //    catch (Exception)
        //    { }
        //    try
        //    {

        //        this.Size = (string)info.GetValue("size", typeof(string));
        //    }
        //    catch (Exception) { }
        //    try
        //    {
        //        this.Height = (string)info.GetValue("height", typeof(string));
        //    }
        //    catch (Exception) { }
        //    try
        //    {
        //        this.Weight = (int)info.GetValue("weight", typeof(int));
        //    }
        //    catch (Exception) { }
        //    try
        //    {
        //        this.IsAnIt = (bool)info.GetValue("isAnIt", typeof(bool));
        //    }
        //    catch (Exception) { }
        //    try
        //    {
        //        this.IsMany = (bool)info.GetValue("isMany", typeof(bool));
        //    }
        //    catch (Exception) { }
        //    try
        //    {
        //        this.Colour = (string)info.GetValue("colour", typeof(string));
        //    }
        //    catch (Exception) { }
        //    try
        //    {
        //        this.LinkedCharacterName = (string)info.GetValue("linkedCharacterName", typeof(string));
        //    }
        //    catch (Exception) { }
        //}

        //override void GetObjectData(SerializationInfo info, StreamingContext context)

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (!String.IsNullOrEmpty(this.FirstName)) info.AddValue("firstName", this.FirstName);
            if (!String.IsNullOrEmpty(this.MiddleName)) info.AddValue("middleName", this.MiddleName);
            if (!String.IsNullOrEmpty(this.FamilyName)) info.AddValue("familyName", this.FamilyName);
            if (!String.IsNullOrEmpty(this.ScriptId)) info.AddValue("scriptId", this.ScriptId);
            info.AddValue("is_important", this.IsImportant);
            info.AddValue("isMany", this.IsMany);
            info.AddValue("isAnIt", this.IsAnIt);
            info.AddValue("colour", this.Colour);
            info.AddValue("height", this.Height);
            info.AddValue("weight", this.Weight);
            info.AddValue("size", this.Size);
            info.AddValue("hasVagina", this.HasVagina);
            info.AddValue("age", this.Age);
            info.AddValue("linkedCharacterName", this.LinkedCharacterName);
            info.AddValue("Pronoun", this.Pronouns);

        }


        public int CompareTo(object obj)
        {
            if (obj == null)
                return -1;
            if (!(obj is Character))
                return -1;
            if (obj.Equals(this))
                return 0;

            //string formatter = "{0}, {1} {2}";
            //string thisObject = String.Format(formatter, this.FamilyName, this.FirstName, this.MiddleName);
            //string otherObject = String.Format(formatter, ((Human)obj).FamilyName, ((Human)obj).FirstName, ((Human)obj).MiddleName);

            string formatter = "{0}, {1} {2}";
            string thisObject = String.Format(formatter, this.FirstName, this.FamilyName, this.MiddleName);
            string otherObject = String.Format(formatter, ((Character)obj).FirstName, ((Character)obj).FamilyName, ((Character)obj).MiddleName);


            return thisObject.CompareTo(otherObject); ;
        }

        public DBbase getDBObejct(DBbase d)
        {
            DBCharacters c = new DBCharacters();
            
            //var c = new DBCharacters();
            c.scriptId = this.ScriptId;
            c.middleName = this.MiddleName;
            c.firstName = this.FirstName;
            c.familyName = this.FamilyName;
            c.colour = this.Colour;
            c.height = this.Height;
            c.weight = this.Weight;
            c.size = this.Size;
            c.hasVagina = this.HasVagina ? 1 : 0;
            c.id = this.Id;
            c.age = this.Age;
            c.ownerId = this.OwnerId;
            c.description = this.Descrption;
            c.is_important = this.IsImportant ? 1 : 0;
            c.isAnIt = this.IsAnIt ? 1 : 0;
            c.isMany = this.IsMany ? 1 : 0;
            // a linked character is a character that takes 2 or more forms. This is used to ensure POV consistancy. 
            // I.e. On character is set on 2nd person, and the same character but in the past will remain in 3rd if this system is not implemented.
            // It refers to the base character. Generally, the character should be loaded in the story, as the two characters co exist in the same story.
            // Intention: Fetch the pov of the reference character, and set it on this one.
            c.linkedCharacterId = this.LinkedCharacterId; 
            c.linkedCharacterName = this.LinkedCharacterName;
            c.Perspective = this.Perspective;
            c.Pronouns = this.Pronouns;
            return c;
        }

        public void setFromDBObject(DBbase d)
        {
            if (d is DBCharacters)
            {
                var c = d as DBCharacters;
                this.ScriptId = c.scriptId;
                this.OwnerId = c.ownerId;
                this.MiddleName = c.middleName;
                this.Id = c.id;
                this.Colour = c.colour;
                this.Weight = c.weight;
                this.Height = c.height;
                this.HasVagina = c.hasVagina == 1 ? true : false;
                this.FirstName = c.firstName;
                this.FamilyName = c.familyName;
                this.Descrption = c.description;
                this.Size = c.size;
                this.Age = c.age;
                this.IsImportant = c.is_important == 1 ? true : false;
                this.IsAnIt = c.isAnIt == 1 ? true : false;
                this.IsMany = c.isMany == 1 ? true : false;
                this.LinkedCharacterId = c.linkedCharacterId;
                this.LinkedCharacterName = c.linkedCharacterName;
                this.Perspective = c.Perspective;
                this.Pronouns = c.Pronouns;

            }
        }

        public override string ToString()
        {
            if (this.FirstName.Trim().ToLower().Equals(this.ScriptId.Trim().ToLower()))
                return this.FirstName;
            return this.FirstName + " (" + this.ScriptId + ")";
        }

        public override bool Equals(System.Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            Character p = obj as Character;
            if ((System.Object)p == null)
            {
                return false;
            }

            // check for reference equality.
            if (System.Object.ReferenceEquals(this, p)) return true;

            // Return true if the fields match:

            return (this.ScriptId.Equals(p.ScriptId, StringComparison.OrdinalIgnoreCase));
        }



        public void setId(long id)
        {
            this.Id = (int)id;
        }
    }
}

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Runtime.Serialization;

//namespace StoryHelper.Classes
//{
//    public class UserCharacter : JSerializable<UserCharacter>, IComparable, DBActionable
//    {
//        public string _firstName = "";
//        public string _middleName = "";
//        public string _familyName = "";
//        public string _height = "";
//        public string _colour = "";

//        public string scriptId = "";

//        public List<UserCharacter> listeners = new List<UserCharacter>();
//        public Pronouns pronouns = new Pronouns(Pronouns.Pronoun.It);

//        public int id { get; set; }
//        public int userId { get; set; }
//        public int charactersId { get; set; }
//        public int storiesId { get; set; }
//        private int perspectiveId = 3;
//        public int PerspectiveId 
//        {
//            get
//            {
//                return perspectiveId;
//            }
//            set
//            {
//                perspectiveId = value;
//            }
//        }
//        public int age { get; set; }
//        public int weight { get; set; }
//        public bool isAnIt { get; set; }
//        public bool isMany { get; set; }

//        public Human belongsTo = null;

//        public string firstName 
//        {
//            get
//            {
//                if (String.IsNullOrWhiteSpace(this._firstName)) return "";
//                return this._firstName;
//            }
//            set
//            {
//                this._firstName = value;
//            }
//        }
//        public string middleName
//        {
//            get
//            {
//                if (String.IsNullOrWhiteSpace(this._middleName)) return "";
//                return this._middleName;
//            }
//            set
//            {
//                this._middleName = value;
//            }
//        }
//        public string familyName
//        {
//            get
//            {
//                if (String.IsNullOrWhiteSpace(this._familyName)) return "";
//                return this._familyName;
//            }
//            set
//            {
//                this._familyName = value;
//            }
//        }
//        public bool hasVagina { get; set; }
//        public string height
//        {
//            get
//            {
//                if (String.IsNullOrWhiteSpace(this._height)) return "";
//                return this._height;
//            }
//            set
//            {
//                this._height = value;
//            }
//        }
//        public string colour
//        {
//            get
//            {
//                if (String.IsNullOrWhiteSpace(this._colour)) return "";
//                return this._colour;
//            }
//            set
//            {
//                this._colour = value;
//            }
//        }

//        public UserCharacter()
//        {
//            id = -1;
//            userId = -1;
//            storiesId = -1;
//            charactersId = -1;
//            PerspectiveId = -1;

//            weight = 0;
//            age = 18;

//            firstName = "";
//            middleName = "";
//            familyName = "";
//            hasVagina = true;
//            height = "";
//            colour = "";
//            isAnIt = false;
//            isMany = false;


//        }

//        public UserCharacter(int userId) : this()
//        {
//            this.userId = userId;
//        }

        

//        public UserCharacter(int userId, string firstName, string middleName, string familyName, bool hasVagina)
//            : this(userId)
//        {
//            this.firstName = firstName;
//            this.familyName = familyName;
//            this.middleName = middleName;
//            this.hasVagina = hasVagina;
//        }

//        public UserCharacter(int userId, Character character) : this()
//        {
//            this.firstName = character.firstName;
//            this.familyName = character.familyName;
//            this.middleName = character.middleName;
//            this.hasVagina = character.hasVagina;
//            this.height = character.height;
//            this.isAnIt = character.isAnIt;
//            this.isMany = character.isMany;
//            this.colour = character.colour;
//            this.age = character.age;
//            this.scriptId = character.scriptId;
//            this.userId = userId;
//        }

//        public UserCharacter(int userId, Character character, Pronouns pronoun) : this(userId, character)
//        {
//            this.pronouns = pronoun;
//        }

//        public void set(Human human)
//        {
//            this.firstName = human.FirstName;
//            this.middleName = human.MiddleName;
//            this.familyName = human.FamilyName;
//            this.colour = human.Colour;
//            this.charactersId = human.charactersId;
//            this.age = human.Age;
//            this.hasVagina = human.HasVagina;
//            this.height = human.Height;
//            this.isAnIt = human.IsAnIt;
//            this.isMany = human.IsMany;
//            //this.userId = human.userId;
//            this.weight = human.Weight;
//        }

//        public static T ObjectFromJson<T>(string json) where T : UserCharacter
//        {
//            try
//            {
//                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
//            }
//            catch (Exception)
//            {

//                return null;
//            }
//        }

//        public string getJson()
//        {
//            string muhString = Newtonsoft.Json.JsonConvert.SerializeObject(this);
//            return muhString;
//        }

//        public UserCharacter getClone()
//        {
//            DBUserCharacter dbMatter = new DBUserCharacter();
//            dbMatter = this.getDBObejct(dbMatter) as DBUserCharacter;
//            UserCharacter h = new UserCharacter();
//            h.setFromDBObject(dbMatter);

//            return h;
//        }

//        public UserCharacter(SerializationInfo info, StreamingContext ctxt)
//        {
//            try
//            {
//                this.firstName = (string)info.GetValue("firstName", typeof(string));
//            }
//            catch (Exception)
//            { }
//            try
//            {
//                this.middleName = (string)info.GetValue("middleName", typeof(string));
//            }
//            catch (Exception)
//            { }
//            try
//            {
//                this.familyName = (string)info.GetValue("familyName", typeof(string));
//            }
//            catch (Exception)
//            { }
//            try
//            {
//                this.hasVagina = (bool)info.GetValue("hasVagina", typeof(bool));
//            }
//            catch (Exception)
//            { }
//            try
//            {
//                this.age = (int)info.GetValue("age", typeof(int));
//            }
//            catch (Exception)
//            { }
//            try
//            {
//                this.isAnIt = (bool)info.GetValue("isAnIt", typeof(bool));
//            }
//            catch (Exception)
//            { }
//            try
//            {
//                this.isMany = (bool)info.GetValue("isMany", typeof(bool));
//            }
//            catch (Exception)
//            { }
//            try
//            {
//                this.PerspectiveId = (int)info.GetValue("perspectiveId", typeof(int));
//            }
//            catch (Exception)
//            { }
//            try
//            {
//                this.height = (string)info.GetValue("height", typeof(string));
//            }
//            catch (Exception)
//            { }
//            try
//            {
//                this.colour = (string)info.GetValue("colour", typeof(string));
//            }
//            catch (Exception)
//            { }
//            try
//            {
//                this.weight = (int)info.GetValue("weight", typeof(int));
//            }
//            catch (Exception)
//            { }
//            try
//            {
//                this.userId = (int)info.GetValue("userId", typeof(int));
//            }
//            catch (Exception)
//            { }
//            try
//            {
//                string pronoun = (string)info.GetValue("Pronoun", typeof(string));
//                this.setPronoun(pronoun);
//            }
//            catch (Exception)
//            {
//            }
//        }

//        //override void GetObjectData(SerializationInfo info, StreamingContext context)

//        public void GetObjectData(SerializationInfo info, StreamingContext context)
//        {
//            if (!String.IsNullOrEmpty(this.familyName)) info.AddValue("firstName", this.firstName);
//            if (!String.IsNullOrEmpty(this.middleName)) info.AddValue("middleName", this.middleName);
//            if (!String.IsNullOrEmpty(this.familyName)) info.AddValue("familyName", this.familyName);
//            if (!String.IsNullOrEmpty(this.familyName)) info.AddValue("colour", this.colour);
//            if (!String.IsNullOrEmpty(this.familyName)) info.AddValue("weight", this.weight);
//            info.AddValue("height", this.height);
//            info.AddValue("hasVagina", this.hasVagina);
//            info.AddValue("age", this.age);
//            info.AddValue("isAnIt", this.isAnIt);
//            info.AddValue("isMany", this.isMany);
//            info.AddValue("perspectiveId", this.PerspectiveId);
//            info.AddValue("Pronoun", this.pronouns.selectedPronoun.ToString());
            

//        }

//        public void setPronoun(Pronouns.Pronoun pronoun)
//        {
//            this.pronouns = new Pronouns(pronoun);
//            if (!pronoun.Equals(Pronouns.Pronoun.It))
//            {
//                int x = 0;
//            }
//        }

//        public void setPronoun(string pronoun)
//        {
//            if (String.IsNullOrWhiteSpace(pronoun)) return;
//            try
//            {
//                var pronounEnum = Pronouns.getPronounFromString(pronoun);
//                this.setPronoun(pronounEnum);
//            }
//            catch (Exception ex)
//            {

//            }
//        }


//        public int CompareTo(object obj)
//        {
//            if (obj == null)
//                return -1;
//            if (!(obj is UserCharacter))
//                return -1;
//            if (obj.Equals(this))
//                return 0;

//            string formatter = "{0}, {1} {2}";
//            string thisObject = String.Format(formatter, this.firstName, this.familyName, this.middleName);
//            string otherObject = String.Format(formatter, ((Character)obj).firstName, ((Character)obj).familyName, ((Character)obj).middleName);


//            return thisObject.CompareTo(otherObject); ;
//        }

//        public DBbase getDBObejct(DBbase d)
//        {
//            DBUserCharacter h = new DBUserCharacter();
            
//            h.middleName = this.middleName;
//            h.firstName = this.firstName;
//            h.familyName = this.familyName;
//            h.hasVagina = this.hasVagina ? 1 : 0;
//            h.id = this.id;
//            h.age = this.age;
//            h.userId = this.userId;



//            h.weight = this.weight;
//            h.colour = this.colour;
//            h.height = this.height;
//            h.isAnIt = this.isAnIt ? 1 : 0;
//            h.isMany = this.isMany ? 1 : 0;
//            h.perspectiveId = this.PerspectiveId;
//            h.charactersId = this.charactersId;
//            h.storiesId = this.storiesId;

//            h.pronoun = this.pronouns.selectedPronoun.ToString();

//            return h;
//        }

//        public void setFromDBObject(DBbase d)
//        {
//            if (d is DBUserCharacter)
//            {
//                var h = d as DBUserCharacter;
//                this.userId = h.userId;
//                this.middleName = h.middleName;
//                this.id = h.id;
//                this.hasVagina = h.hasVagina == 1 ? true : false;
//                this.firstName = h.firstName;
//                this.familyName = h.familyName;
//                this.age = h.age;


//                this.storiesId = h.storiesId;
//                this.charactersId = h.charactersId;
//                this.PerspectiveId = h.perspectiveId;
//                this.isMany = h.isMany == 1 ? true : false;
//                this.isAnIt = h.isAnIt == 1 ? true : false;
//                this.height = h.height;
//                this.colour = h.colour;
//                this.weight = h.weight;

//                this.setPronoun(h.pronoun);

//            }
//        }

//        public override string ToString()
//        {
//            return this.firstName;
//        }

//        public override bool Equals(System.Object obj)
//        {
//            // If parameter is null return false.
//            if (obj == null)
//            {
//                return false;
//            }

//            // If parameter cannot be cast to Point return false.
//            UserCharacter p = obj as UserCharacter;
//            if ((System.Object)p == null)
//            {
//                return false;
//            }

//            // check for reference equality.
//            if (System.Object.ReferenceEquals(this, p)) return true;

//            // Return true if the fields match:

//            return
//                (this.middleName.Equals(p.middleName)) &&
//                (this.familyName.Equals(p.familyName)) &&
//                (this.firstName.Equals(p.firstName)) &&
//                (this.age == p.age) &&
//                (this.userId == p.userId) &&
//                (this.hasVagina == p.hasVagina);
//        }



//        public void setId(long id)
//        {
//            this.id = (int)id;
//        }
//    }
//}

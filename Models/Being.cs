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
    public abstract class Being : Matter 
    {
        public Being()
            : base()
        {
            setClassConstants();
        }

        public bool age_readonly { get; set; }
        public bool height_readonly { get; set; }


        public Being(string name) : base(name) {
            setClassConstants();
        }

        public Being(string name, string spieces) : base(name) {
            this.Spieces = spieces;
            setClassConstants();
        }

        public Being(string name, int age) : base(name) {
            this.Age = age;
            setClassConstants();
        }

        public Being(string name, string spieces, int age) : base(name) {
            this.Spieces = spieces;
            this.Age = age;
            setClassConstants();
        }

        private string spieces = "";
        public string Spieces
        {
            get
            {
                //if (!this.spiecesGiven) throw new Exception("Species not set!");
                return spieces;
            }
            set
            {
                spieces = value;
                this.spiecesGiven = true;
            }
        }
        private bool spiecesGiven = false;

        public bool SpiecesGiven
        {
            get { return spiecesGiven; }
        }

        private int age = 0;
        public int Age
        {
            get { return age; }
            set {
                //if (age < 0) throw new Exception("Age cannot be less than zero!");
                
                age = value; 
            }
        }

        

        private string height = "";
        public string Height
        {
            get { return height; }
            set {
                
                height = value; 
            }
        }

        public override ActionParser getClone()
        {
            return this.deepCopy();
            //return (ActionParser)Newtonsoft.Json.JsonConvert.DeserializeObject<Being>(this.getJson());
        }

        public override ActionParser deepCopy()
        {
            ActionParser b = base.deepCopy();

            Matter c = copy(b as Matter);

            return c as ActionParser;
        }

        protected override Matter copy(Matter m)
        {
            Console.WriteLine(m.Name + " = Being accessed");
            Being c = m as Being;
            c.spieces = this.spieces;
            c.spiecesGiven = this.spiecesGiven;
            c.height = this.height;
            c.age = this.age;

            return base.copy(c);
        }

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Age", this.Age);
            if (!String.IsNullOrEmpty(this.Spieces)) info.AddValue("Spieces", this.Spieces);
            if (!String.IsNullOrEmpty(this.Height)) info.AddValue("Height", this.Height);
        }

        //Deserialization constructor.
        public Being(SerializationInfo info, StreamingContext ctxt)
            : base(info, ctxt)
        {
            //Get the values from info and assign them to the appropriate properties
            try{ this.Age = (int)info.GetValue("Age", typeof(int)); } catch (Exception) { }
            try { this.Spieces = (string)info.GetValue("Spieces", typeof(string)); } catch (Exception) { }
            try { this.Height = (string)info.GetValue("Height", typeof(string)); }
            catch (Exception) { }

        }

        public override int CompareTo(object obj)
        {
            return base.CompareTo(obj);
        }

        protected abstract void setClassConstants();

        protected override void setDelegates()
        {
            base.setDelegates();
            addDelegate("AGE", "", null, (x, y) => { return this.Age.ToString(); });
            addDelegate("AGE-N", "", null, (x, y) => { return this.Age.ToString(); });
            addDelegate("AGE-W", "", null, (x, y) => { return this.getAgeInWordForm().Trim(); });
            addDelegate("AGE-WH", "", null, (x, y) => { return this.getAgeInWordForm().Trim().Replace(' ', '-'); });
            addDelegate("TYPE", "", null, (x, y) => { 
                return this.Spieces; 
            });
            addDelegate("HE", "", null, (x, y) => { return this.Height; });
        }

        public override string interpret(string code, int tense)
        {

            return base.interpret(code, tense);

            
            switch (code.ToUpper())
            {
                case "AGE":
                    return this.Age.ToString();
                case "AGE-N":
                    return this.Age.ToString();
                case "AGE-W":
                    return this.getAgeInWordForm().Trim();
                case "AGE-WH":
                    return this.getAgeInWordForm().Trim().Replace(' ', '-');
                case "TYPE":
                    return this.Spieces;
                case "HE":
                    return this.Height;
                default:
                    return base.interpret(code, tense);
            }


        }

        public override Dictionary<string, string> help()
        {
            Dictionary<string, string> d = base.help();
            d.Add("AGE","Gets the age in number form.");
            d.Add("AGE-N", "Gets the age in number form.");
            d.Add("AGE-W", "Gets the age in word form.");
            d.Add("AGE-WH", "Gets the age in word form seperated by hyphens.");
            d.Add("SP", "Gets the type of species this being is.");
            d.Add("HE", "Gets the height.");
            return d;
        }

        public override void selfRegisterAll()
        {
            base.selfRegisterAll();

        }

        #region AGE METHODS
        private string getAgeInWordForm()
        {

            char[] ageChar = this.Age.ToString().ToCharArray();
            int[] age = new int[ageChar.Length];

            for(int x =  0; x < ageChar.Length; x++)
            {
                age[x] = int.Parse(ageChar[x].ToString());
            }
            
            switch (ageChar.Length)
            {
                case 0:
                    return "a few months";
                case 1:
                    return this.getSingleDigit(this.Age);
                case 2:
                    return this.getSecondsDigit(age[0], age[1]);
                case 3:
                    return this.getThreeDigit(age[0], age[1], age[2]);
                case 4:
                    return getFourDigit(age);
                case 5:
                    return getFiveDigit(age);
                case 6:
                    return getSixDigit(age);
                case 7:
                    return getSevenDigits(age);
                case 8:
                    return getEightDigits(age);
                case 9:
                    return getNineDigits(age);
            }

            return "no age";

        }

        private string getSingleDigit(int value) {

            switch (value)
            {
                case 1:
                    return "one";
                case 2:
                    return "two";
                case 3:
                    return "three";
                case 4:
                    return "four";
                case 5:
                    return "five";
                case 6:
                    return "six";
                case 7:
                    return "seven";
                case 8:
                    return "eight";
                case 9:
                    return "nine";
                default:
                    return "";
            }
            
        }

        private string getTeensDigit(int value)
        {

            switch (value)
            {
                case 1:
                    return "eleven";
                case 2:
                    return "twelve";
                case 3:
                    return "thirteen";
                case 4:
                    return "fourteen";
                case 5:
                    return "fifteen";
                case 6:
                    return "sixteen";
                case 7:
                    return "seventeen";
                case 8:
                    return "eighteen";
                case 9:
                    return "ninteen";
                default:
                    return "ten";
            }

        }

        private string getSecondsDigit(int second, int firstPlaceDigit)
        {
            switch (second)
            {
                case 1:
                    return getTeensDigit(firstPlaceDigit);
                case 2:
                    return "twenty " + getSingleDigit(firstPlaceDigit);
                case 3:
                    return "thirty " + getSingleDigit(firstPlaceDigit);
                case 4:
                    return "fourty " + getSingleDigit(firstPlaceDigit);
                case 5:
                    return "fifty " + getSingleDigit(firstPlaceDigit);
                case 6:
                    return "sixty " + getSingleDigit(firstPlaceDigit);
                case 7:
                    return "seventy " + getSingleDigit(firstPlaceDigit);
                case 8:
                    return "eighty " + getSingleDigit(firstPlaceDigit);
                case 9:
                    return "ninety " + getSingleDigit(firstPlaceDigit);
                default:
                    return getSingleDigit(firstPlaceDigit);
            }
        }

        private string getThreeDigit(int hundredth, int tenth, int first )
        {
            return (((hundredth != 0) ? getSingleDigit(hundredth) + " hundred " : "") + getSecondsDigit(tenth, first)).Trim();
        }

        private string getFourDigit(int[] age)
        {
            return (((age[age.Length - 4] != 0) ? getSingleDigit(age[age.Length - 4]) + " thousand " : "") + getThreeDigit(age[age.Length - 3], age[age.Length - 2], age[age.Length - 1])).Trim();
        }

        private string getFiveDigit(int[] age)
        {
            return ((age[age.Length - 5] != 0) ? getSecondsDigit(age[age.Length - 5], age[age.Length - 4]) : getSingleDigit(age[age.Length - 4])) + " thousand " + getThreeDigit(age[age.Length - 3], age[age.Length - 2], age[age.Length - 1]).Trim();
        }

        private string getSixDigit(int[] age)
        {
            return ((age[age.Length - 6] != 0) ? getThreeDigit(age[age.Length - 6], age[age.Length - 5], age[age.Length - 4]) : getSecondsDigit(age[age.Length - 5], age[age.Length - 4])) + " thousand " + getThreeDigit(age[age.Length - 3], age[age.Length - 2], age[age.Length - 1]).Trim();
        }

        private string getSevenDigits(int[] age)
        {
            return ((age[age.Length - 7] != 0) ? getSingleDigit(age[age.Length - 7]) + " million " : "") + getSixDigit(age).Trim();
        }

        private string getEightDigits(int[] age)
        {
            return ((age[age.Length - 8] != 0) ? getSecondsDigit(age[age.Length - 8], age[age.Length - 7]) : getSingleDigit(age[age.Length - 7])) + " million " + getSixDigit(age).Trim();
        }

        private string getNineDigits(int[] age)
        {
            return ((age[age.Length - 9] != 0) ? getThreeDigit(age[age.Length - 9], age[age.Length - 8], age[age.Length - 7]) : getSecondsDigit(age[age.Length - 8], age[age.Length - 7])) + " million " + getSixDigit(age).Trim();
        }

        public override DBbase getDBObejct(DBbase o)
        {
            return base.getDBObejct(o);
        }

        public override void setFromDBObject(DBbase o)
        {
            base.setFromDBObject(o);
        }

        #endregion

    }
}

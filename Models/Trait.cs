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
    public class Trait : Matter
    {
        public Trait() : base() 
        {
            this.IsUsed = false;
        }

        public Trait(string name) : base(name) 
		{
            this.Id = name.Replace(" ", "_");
			//this.IsUsed = false;
		}

        public override ActionParser deepCopy()
        {
            Trait t = new Trait(this.Name);
            this.copy(t);

            return t as ActionParser;
        }

        protected override Matter copy(Matter m)
        {
            Trait t = m as Trait;

            return base.copy(t);
        }

        public override ActionParser getClone()
        {

            DBTrait dbMatter = new DBTrait();
            this.getDBObejct(dbMatter);
            Trait m = new Trait();
            m.setFromDBObject(dbMatter);
            return m;
            //return this.deepCopy();
            //return (ActionParser)Newtonsoft.Json.JsonConvert.DeserializeObject<Trait>(this.getJson());
        }

        protected override void setDelegates()
        {
            base.setDelegates();
            var d = getFreshParameterList();


            
            addDelegate("MY", "", null, (x, y) => { 
                if (this.Owner != null && this.Owner is Matter)
                    return (this.Owner as Matter).interpret("PA", tense);
                return this.interpret("PA", tense);
            });
        }


        public override string interpret(string code, int tense)
        {
            return base.interpret(code, tense);


            Verb.Gender gender = Verb.Gender.it;
            string verb = Verb.getVerb(code, this.IsMany, 3, true, gender, tense);
            if (!String.IsNullOrWhiteSpace(verb)) return verb;

            switch (code.ToUpper())
            {
                case "PP":
                    return this.getPossesivePronoun();
                case "PPP":
                    return this.Name + "'s";
                case "SP":
                    return this.getSubjectPronoun();
                case "SPP":
                    return this.Name;
                case "OP":
                    return this.getObjectPronouns();
                case "OPP":
                    return this.Name;
                case "PAP":
                    return this.Name + "'s";
                case "MY":
                    if (this.Owner != null && this.Owner is Matter)
                        return (this.Owner as Matter).interpret("PA", tense);
                    return base.interpret(code, tense);
                default:
                    return base.interpret(code, tense);
            }
        }

        public override Dictionary<string, string> help()
        {
            Dictionary<string, string> dic = base.help();
            dic.Add("G", "Returns the groupId number.");
            dic.Add("MY", "Returns the parent pa code.");
            dic.Add("PP", "Gets a personal possesive pronoun.");
            dic.Add("SP", "Gets a subject pronoun.");
            dic.Add("OP", "Gets an object pronoun.");
            dic.Add("PA", "Gets a possessive adjective pronoun.");
            dic.Add("RP", "Gets a reflexive pronoun.");
            

            return dic;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

        }


        //Deserialization constructor.
        public Trait(SerializationInfo info, StreamingContext ctxt)
            : base(info, ctxt)
        {

        }

         public override int CompareTo(object obj)
         {
             return base.CompareTo(obj);
         }

         public override DBbase getDBObejct(DBbase o)
         {
             DBTrait t = o as DBTrait;
             if (t == null) t = new DBTrait();
             
             t.scriptId = this.Id;
             t.name_readonly = this.name_readonly ? 1 : 0;

             

             return base.getDBObejct(t);
         }

         public override void setFromDBObject(DBbase o)
         {
             DBTrait t = o as DBTrait;
             if (t == null) t = new DBTrait();
             base.setFromDBObject(t);
             this.setId(t.scriptId);
             this.name_readonly = t.name_readonly == 1 ? true : false;
         }

         

         public override bool Equals(System.Object obj)
         {
             // If parameter is null return false.
             if (obj == null)
             {
                 return false;
             }

             // If parameter cannot be cast to Point return false.
             Trait p = obj as Trait;
             if ((System.Object)p == null)
             {
                 return false;
             }

             // check for reference equality.
             if (System.Object.ReferenceEquals(this, p)) return true;

             // Return true if the fields match:

             return 
                // (this.IsMany == p.IsMany) &&
                // (this.getFullId().Equals(p.getFullId())) &&
                // this.Name.Equals(p.Name) &&
                //this.Other.Equals(p.Other) &&
                //this.Size.Equals(p.Size) &&
                //this.Weight.Equals(p.Weight) &&
                //this.GroupId == p.GroupId &&
                //this.IsMany == p.IsMany &&
                //this.IsUsed == p.IsUsed &&
                this.Id.ToLower().Equals(p.Id.ToLower());
         }

         public void combine(Trait other)
         {
             if (other == null) return;
             foreach (var item in other.Adjectives) this.AddAdjective(item);
             foreach (var item in other.Adverbs) this.addAdverb(item);
             foreach (var item in other.Aliases) this.AddAlias(item);
             foreach (var item in other.States) this.addState(item);
             foreach (var item in other.Who) this.addWho(item);


             if (!String.IsNullOrWhiteSpace(other.Colour))
             {
                 this.Colour = other.Colour;
             }

             if (!String.IsNullOrWhiteSpace(other.Size))
             {
                 this.Size = other.Size;
             }
         }

         public static Trait createConjunction(string name, string condition, string outcome1, string outcome2)
         {
             Trait t = new Trait("c_" + name.Replace(" ", "_"));
             t.GroupId = 71;
             t.AddAdjective("[owner." + condition + "?[owner." + outcome1 + ".A-A]:[owner." + outcome2 + ".A-A]]");
             t.addAdverb("[owner." + condition + "?[owner." + outcome1 + ".V-A]:[owner." + outcome2 + ".V-A]]");
             t.AddAlias("[owner." + condition + "?[owner." + outcome1 + ".K-A]:[owner." + outcome2 + ".K-A]]");
             t.addState("[owner." + condition + "?[owner." + outcome1 + ".S-A]:[owner." + outcome2 + ".S-A]]");
             t.addWho("[owner." + condition + "?[owner." + outcome1 + ".F-A]:[owner." + outcome2 + ".F-A]]");
             t.Size =   ("[owner." + condition + "?[owner." + outcome1 + ".Z]:[owner." + outcome2 + ".Z]]");
             t.Colour = ("[owner." + condition + "?[owner." + outcome1 + ".C]:[owner." + outcome2 + ".C]]");
             

             return t;

         }

    }
}

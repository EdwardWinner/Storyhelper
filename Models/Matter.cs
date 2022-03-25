using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Linq.Expressions;
using System.Reflection;
using StoryHelper.Classes;
using StoryHelperLibrary.Helpers;
using Newtonsoft.Json;

namespace StoryHelper.Classes
{

    public abstract class PropertySetable
    {

        protected const string regexSetProperty = "^get[a-zA-z]+\\(\\)$";
        protected const string regexGetProperty = @"^set[a-zA-z]+\((\d+|[a-zA-Z .!?_]+)\)$";
        protected readonly Regex regexSet = new Regex(regexSetProperty, RegexOptions.IgnoreCase);
        protected readonly Regex regexGet = new Regex(regexGetProperty, RegexOptions.IgnoreCase);
        

        protected string setProperty(string codeFunctionName)
        {
            if (regexSet.IsMatch(codeFunctionName))
            {
                string stringPropertyName = codeFunctionName.Remove(0, 3);
                int start = stringPropertyName.IndexOf('(');
                if (start == -1) return codeFunctionName;
                stringPropertyName = stringPropertyName.Substring(0, start);
                string stringNumber = codeFunctionName.Replace("set" + stringPropertyName + "(", "");
                stringNumber = stringNumber.Remove(stringNumber.Length - 1, 1);
                int value = 100;
                bool success = int.TryParse(stringNumber, out value);
                if (success)
                {
                    success = setPropertyValue(stringPropertyName, value);
                    if (success)
                        return "";
                    else
                        return codeFunctionName;
                }
                else
                {
                    success = setPropertyValue(stringPropertyName, stringNumber);
                    if (success)
                        return "";
                    else
                        return codeFunctionName;
                }
            }

            return codeFunctionName;
        }

        protected string getProperty(string codeFunctionName)
        {
            if (regexGet.IsMatch(codeFunctionName))
            {
                string stringPropertyName = codeFunctionName.Remove(0, 3);
                int start = stringPropertyName.IndexOf('(');
                if (start == -1) return codeFunctionName;
                stringPropertyName = stringPropertyName.Substring(0, start);
                string success = getPropertyValue(stringPropertyName);
                if (!String.IsNullOrWhiteSpace(success))
                    return success;
                else
                    return "";
            }



            return "";
        }

        protected string getPropertyValue(string propertyName)
        {
            PropertyInfo[] properties = this.GetType().GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (property.Name.Equals(propertyName))
                {
                    try
                    {
                        string value = DBProperties.GetPropertyValue<object>(this, property.Name).ToString();
                        //property.SetValue(this, Convert.ChangeType(propertyValue, property.PropertyType), null);
                        if (String.IsNullOrWhiteSpace(value)) return "";
                        return value;
                    }
                    catch (Exception ex)
                    {
                        return "";
                    }
                }
            }

            return "";
        }

        protected bool setPropertyValue(string propertyName, object propertyValue)
        {
            PropertyInfo[] properties = this.GetType().GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (property.Name.Equals(propertyName))
                {
                    try
                    {
                        property.SetValue(this, Convert.ChangeType(propertyValue, property.PropertyType), null);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        return false;
                    }
                }
            }

            return false;
        }
    }

    public abstract class Commandable : PropertySetable
    {
        bool delegatesIsSet = false;
        public Commandable()
        {
            this.setDelegates();
            delegatesIsSet = true;
        }

        protected string startCommand = "[";
        protected string endCommand = "]";

        public string interpret(string[] code)
        {
            if (!this.delegatesIsSet) throw new Exception("Delegates Not Set!");
            string[] returnValues = new string[code.Length];
            int x = 0;
            foreach (string str in code)
            {
                int index = -1;
                index = str.IndexOf('.');
                if (index > -1)
                {
                    string[] cmds = str.Split(new char[] { '.' });
                    if (cmds.Length < 2)
                    {
                        returnValues[x++] = this.interpret(str);
                        continue;
                    }
                    else
                    {
                        string gather = "";
                        foreach (var cmd in cmds)
                        {
                            gather += this.interpret(cmd) + " ";
                        }
                        returnValues[x++] = gather.Trim();
                        continue;
                    }
                }
                returnValues[x++] = this.interpret(str);
            }

            return String.Join(" ", returnValues);
        }

        public virtual string interpret(string code)
        {
            if (!this.delegatesIsSet) throw new Exception("Delegates Not Set!");
            if (String.IsNullOrWhiteSpace(code)) return "";
            if (code.ToUpper().StartsWith("~")) return code.Remove(0, 1);

            // ensure that all methods are properly parsed
            if (code.IndexOf(".") > -1)
            {
                var listOfMethods = parseCommandMethods(code, ".", "(", ")");
                if (listOfMethods.Count > 1) return this.interpret(listOfMethods.ToArray());
            }

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
        }

        //private string findFirstCommand(string command, out int start, out int end, string startingCharacter, string endingCharacter, bool returnSurroundingCharacters)
        //{
        //    #region Method init
        //    start = -1;
        //    end = -1;
        //    string finalString = "";
        //    if (String.IsNullOrEmpty(command))
        //    {
        //        return "";
        //    }
        //    if (String.IsNullOrEmpty(startingCharacter))
        //    {
        //        return command;
        //    }
        //    if (String.IsNullOrEmpty(endingCharacter))
        //    {
        //        return command;
        //    }
        //    if (startingCharacter.Equals(endingCharacter)) return command;
        //    string str = command;
        //    // check if there is a command to process
        //    int characterLocation = str.IndexOf(startingCharacter);
        //    #endregion


        //    if (characterLocation == -1)
        //    {
        //        return command; ;
        //    }


        //    characterLocation = 0;
        //    int lastPos = characterLocation;
        //    int nestedLevel = 0;

        //    lastPos = characterLocation;
        //    characterLocation = str.IndexOf(startingCharacter, characterLocation);
        //    if (characterLocation == -1)
        //    {

        //        finalString = str.Substring(lastPos, str.Length - lastPos).Replace(startingCharacter, "").Replace(endingCharacter, "");
        //        return finalString;
        //    }

        //    start = characterLocation;


        //    characterLocation += startingCharacter.Length;
        //    int charEndLocation = str.IndexOf(endingCharacter, characterLocation);

        //    int dummyEndLocation = str.IndexOf(endingCharacter, characterLocation);
        //    int dummyStartLocation = str.IndexOf(startingCharacter, characterLocation);
        //    int spotIndex = 0;
        //    int skipLength = 0;

        //    nestedLevel = 1;
        //    do
        //    {
        //        if (dummyStartLocation <= dummyEndLocation && dummyStartLocation != -1)
        //        {
        //            nestedLevel++;
        //            spotIndex = dummyStartLocation;
        //            skipLength = startingCharacter.Length;
        //        }
        //        else
        //        {
        //            nestedLevel--;
        //            spotIndex = dummyEndLocation;
        //            skipLength = endingCharacter.Length;
        //        }

        //        dummyEndLocation = str.IndexOf(endingCharacter, spotIndex + skipLength);
        //        dummyStartLocation = str.IndexOf(startingCharacter, spotIndex + skipLength);
        //        if (dummyEndLocation == -1 || nestedLevel == 0) break;
        //        spotIndex = dummyEndLocation;
        //    } while (nestedLevel > 0);

        //    charEndLocation = spotIndex;
        //    end = charEndLocation;

        //    if (start > end) return str;

        //    finalString = str.Substring((start + startingCharacter.Length), end - (start + startingCharacter.Length));

        //    if (returnSurroundingCharacters)
        //    {
        //        finalString = startingCharacter + finalString + endingCharacter;
        //    }

        //    return finalString;
        //}

        protected string findFirstCommand(string command, out int start, out int end, string startingCharacter, string endingCharacter, bool returnSurroundingCharacters)
        {
            #region Method init
            start = -1;
            end = -1;
            string finalString = "";
            if (String.IsNullOrEmpty(command))
            {
                return "";
            }
            if (String.IsNullOrEmpty(startingCharacter))
            {
                return command;
            }
            if (String.IsNullOrEmpty(endingCharacter))
            {
                return command;
            }
            if (startingCharacter.Equals(endingCharacter)) return command;
            string str = command;
            // check if there is a command to process
            int characterLocation = str.IndexOf(startingCharacter);
            #endregion


            if (characterLocation == -1)
            {
                return command; ;
            }


            characterLocation = 0;
            int lastPos = characterLocation;
            int nestedLevel = 0;

            lastPos = characterLocation;
            characterLocation = str.IndexOf(startingCharacter, characterLocation);
            if (characterLocation == -1)
            {

                finalString = str.Substring(lastPos, str.Length - lastPos).Replace(startingCharacter, "").Replace(endingCharacter, "");
                return finalString;
            }

            start = characterLocation;


            characterLocation += startingCharacter.Length;
            int charEndLocation = str.IndexOf(endingCharacter, characterLocation);

            int dummyEndLocation = str.IndexOf(endingCharacter, characterLocation);
            int dummyStartLocation = str.IndexOf(startingCharacter, characterLocation);
            int spotIndex = 0;
            int skipLength = 0;

            nestedLevel = 1;
            do
            {
                if (dummyStartLocation <= dummyEndLocation && dummyStartLocation != -1)
                {
                    nestedLevel++;
                    spotIndex = dummyStartLocation;
                    skipLength = startingCharacter.Length;
                }
                else
                {
                    nestedLevel--;
                    spotIndex = dummyEndLocation;
                    skipLength = endingCharacter.Length;
                }

                dummyEndLocation = str.IndexOf(endingCharacter, spotIndex + skipLength);
                dummyStartLocation = str.IndexOf(startingCharacter, spotIndex + skipLength);
                if (dummyEndLocation == -1 || nestedLevel == 0) break;
                spotIndex = dummyEndLocation;
            } while (nestedLevel > 0);

            charEndLocation = spotIndex;
            end = charEndLocation;

            if (start > end) return str;

            finalString = str.Substring((start + startingCharacter.Length), end - (start + startingCharacter.Length));

            if (returnSurroundingCharacters)
            {
                finalString = startingCharacter + finalString + endingCharacter;
            }

            return finalString;
        }

        protected string methodParse(string command, out int start, out int end, string methodToken, string characterParameterStarter, string characterParameterEnder, int startingPoint = 0)
        {
            start = 0; end = 0;

            start = command.IndexOf(methodToken, startingPoint);
            if (start == -1) return command;
            if (characterParameterStarter == characterParameterEnder) return command;

            int carriageLocation = start;

            int nestedAmount = 0;
            while (carriageLocation < command.Length - methodToken.Length && (command.Substring(++carriageLocation, methodToken.Length) != methodToken || nestedAmount > 0))
            {
                if (carriageLocation < command.Length - characterParameterStarter.Length && command.Substring(carriageLocation, characterParameterStarter.Length) == characterParameterStarter) nestedAmount++;
                if (carriageLocation < command.Length - characterParameterEnder.Length && command.Substring(carriageLocation, characterParameterEnder.Length) == characterParameterEnder) nestedAmount--;
            }

            if (nestedAmount != 0) return command;
            end = carriageLocation;
            if (carriageLocation - (start + methodToken.Length) < 0) return command.Substring(start + methodToken.Length);
            return command.Substring(start + methodToken.Length, carriageLocation - (start + methodToken.Length));
        }

        protected List<string> parseParameters(string strParameters, string parameterToken, string stringCharacter)
        {
            var parameters = strParameters.Split(new string[] { parameterToken }, StringSplitOptions.None);
            bool isString = false;
            string tempString = "";
            for (int x = parameters.Length - 1; x >= 0; x--)
            {
                if (!isString)
                {
                    if (parameters[x].Trim().EndsWith(stringCharacter) && !parameters[x].Trim().StartsWith(stringCharacter))
                    {
                        isString = true;
                        tempString = parameters[x];
                        parameters[x] = "";
                    }
                }
                else
                {
                    if (parameters[x].Trim().StartsWith(stringCharacter))
                    {
                        parameters[x] += parameterToken + tempString;
                        tempString = "";
                        isString = false;
                    }
                    else
                    {
                        tempString = parameters[x] + parameterToken + tempString;
                    }
                }
            }

            if (isString)
            {
                parameters[0] = tempString;
            }

            List<string> p = new List<string>();
            foreach (string str in parameters)
            {
                if (!String.IsNullOrWhiteSpace(str))
                {
                    p.Add(str);
                }
            }

            return p;

        }

        public List<string> parseCommandMethods(string command, string methodToken, string characterParameterStarter, string characterParameterEnder, int startingPoint = 0)
        {
            int start = 0; int end = 0;
            List<string> list = new List<string>();

            start = command.IndexOf(methodToken, startingPoint);
            if (start == -1 || characterParameterStarter == characterParameterEnder)
            {
                list.Add(command);
                return list;
            }

            if (start > 0) list.Add(command.Substring(0, start));

            int carriageLocation = start;

            while (carriageLocation < command.Length - methodToken.Length)
            {
                string method = methodParse(command, out start, out end, methodToken, characterParameterStarter, characterParameterEnder, carriageLocation);
                if (!String.IsNullOrWhiteSpace(method)) list.Add(method);
                carriageLocation = end;
            }

            return list;
        }

        #region Method Delegate Structure Stuff
        public class MethodPackage
        {
            public string MethodName { get; set; }
            public string Description { get; set; }
            public List<Parameter> ParameterDescriptions = new List<Parameter>();
            public delegate string Method(string methodName, List<string> parameters);
            public Dictionary<string, Method> methods = new Dictionary<string, Method>();
            public void Add(MethodPackage package)
            {
                // overridding behavior.
                for (int x = package.methods.Count - 1; x > -1; x--)
                {
                    bool found = false;
                    for (int y = this.methods.Count - 1; y > -1; y--)
                    {
                        if (package.methods.ElementAt(x).Key.Equals(this.methods.ElementAt(y).Key))
                        {
                            this.methods[this.methods.ElementAt(y).Key] = package.methods.ElementAt(x).Value;
                            found = true;
                            break;
                        }
                    }
                    if(!found) this.methods.Add(package.methods.ElementAt(x).Key, package.methods.ElementAt(x).Value);
                }
                
            }
            public MethodPackage(string methodName, List<Parameter> parameterDescriptions, string description, Method method)
            {
                this.MethodName = methodName;
                this.Description = description;
                this.ParameterDescriptions = parameterDescriptions;
                this.methods.Add(this.getSignature(parameterDescriptions), method);
            }
            private string getSignature(List<Parameter> parameterDescriptions)
            {
                string final = this.MethodName;
                if (parameterDescriptions == null) return final;
                foreach (var p in parameterDescriptions)
                {
                    final += "|" + p.ToString();
                }
                return final;
            }
            private string getSignature(List<string> parameters)
            {
                string final = this.MethodName;
                if (parameters == null) return final;
                foreach (var p in parameters)
                {
                    final += "|" + Parameter.getParameterTypeName(p);
                }
                return final;
            }
            private string getSignature()
            {
                return this.getSignature(this.ParameterDescriptions);
            }
            public string Invoke(List<string> parameters)
            {
                string key = getSignature(parameters);
                try
                {
                    if (methods.ContainsKey(key))
                    {
                        return methods[key].Invoke(this.MethodName, parameters);
                    }
                    return "Error: No method " + key;
                }
                catch (Exception ex)
                {
                    return "Error: No method " + key;
                }
            }
            public override bool Equals(object obj)
            {
                if (obj == null) return false;
                if(!obj.GetType().Equals(typeof(MethodPackage))) return false;
                return (obj as MethodPackage).getSignature().Equals(this.getSignature());
            }
            public override string ToString()
            {
                string final = this.MethodName;
                bool hasParams = this.ParameterDescriptions.Count > 0;
                if (hasParams) final += "(";
                foreach (var para in this.ParameterDescriptions)
                {
                    final += para.ToString() + " " + para.Name.Replace(" ", "") + ", " ;
                }
                if (hasParams) final = final.Substring(0, final.Length - 2);
                return final + (hasParams?")":"");
            }
            public class Parameter : IComparable 
            {
                public string Name { get; set; }
                public string Description { get; set; }
                private Type ParameterType { get; set; }
                private object Value { get; set; }
                public Parameter(string name, string description, object value)
                {
                    this.Name = name;
                    this.Description = description;
                    if (value == null) throw new Exception("Parameter value cannot be null.");
                    this.ParameterType = value.GetType();
                    this.Value = value;
                }

                private Parameter(string name, string description)
                {
                    this.Name = name;
                    this.Description = description;
                }

                public Parameter(string name, string description, Type type): this(name, description)
                {
                    if (type.Equals(typeof(string)) || type.Equals(typeof(int)) || type.Equals(typeof(double)))
                    {
                        this.ParameterType = type;
                    }
                    else
                    {
                        throw new Exception("Parameter type is not allowed.");
                    }
                }

                public Parameter(string name, string description, string value):this(name, description)
                {
                    this.setValue(value);
                }

                public static Parameter getParameter(string value)
                {
                    Parameter p = new Parameter("", "", value);
                    return p;
                }

                public static Type parseParameter(string parameter, out object parsed)
                {
                    parsed = null;
                    if (String.IsNullOrWhiteSpace(parameter))
                    {
                        parsed = parameter;
                        return typeof(string);
                    }
                    int number = -1;
                    if (int.TryParse(parameter, out number))
                    {
                        parsed = number;
                        return typeof(int);
                    }

                    double number2 = -1;
                    if (double.TryParse(parameter, out number2))
                    {
                        parsed = number2;
                        return typeof(double);
                    }

                    parsed = parameter;
                    return typeof(string);
                    
                }

                public static Type getParameterType(string value)
                {
                    if (String.IsNullOrWhiteSpace(value))
                    {
                        return typeof(string);
                    }
                    int number = -1;
                    if (int.TryParse(value, out number))
                    {
                        return typeof(int);
                    }

                    double number2 = -1;
                    if (double.TryParse(value, out number2))
                    {
                        return typeof(double);
                    }

                    return typeof(string);
                }

                public static string getParameterTypeName(string value)
                {
                    if (String.IsNullOrWhiteSpace(value))
                    {
                        return "string";
                    }
                    int number = -1;
                    if (int.TryParse(value, out number))
                    {
                        return "int";
                    }

                    double number2 = -1;
                    if (double.TryParse(value, out number2))
                    {
                        return "double";
                    }

                    return "string";
                }

                public Type getType()
                {
                    return this.ParameterType;
                }

                public void setValue(string value)
                {
                    if (String.IsNullOrWhiteSpace(value))
                    {
                        this.Value = "";
                        this.ParameterType = typeof(string);
                        return;
                    }
                    int number = -1;
                    if (int.TryParse(value, out number))
                    {
                        this.ParameterType = typeof(int);
                        this.Value = number;
                        return;
                    }

                    double number2 = -1;
                    if (double.TryParse(value, out number2))
                    {
                        this.ParameterType = typeof(double);
                        this.Value = number2;
                        return;
                    }

                    this.ParameterType = typeof(string);
                    this.Value = value;
                }

                public dynamic getValue()
                {
                    return Convert.ChangeType(this.Value, this.ParameterType);
                }

                public int CompareTo(object obj)
                {
                    if (obj == null)
                        return 1;
                    if (!(obj is Parameter))
                        return -1;
                    if ((obj as Parameter).Equals(this))
                        return 0;
                    return this.Name.CompareTo(((Parameter)obj).Name);
                }

                public override bool Equals(object obj)
                {
                    if(obj.GetType().Equals(this.GetType()))
                    {
                        return (obj as Parameter).ParameterType.Equals(this.ParameterType);
                    }
                    if (this.ParameterType.Equals(typeof(string)))
                    {
                        return true;
                    }
                    return obj.GetType().Equals(this.ParameterType);

                    //if(!isSame) return false;
                    //isSame &= (obj as Parameter).Name.Equals(this.Name);
                    //isSame &= (obj as Parameter).ParameterType.Equals(this.ParameterType);

                }

                public override string ToString()
                {
                    if(this.ParameterType.Equals(typeof(string)))
                    {
                        return "string";
                    }
                    if (this.ParameterType.Equals(typeof(int)))
                    {
                        return "int";
                    }
                    if (this.ParameterType.Equals(typeof(double)))
                    {
                        return "double";
                    }

                    return "string";
                }
            }
        }

        public Dictionary<string, MethodPackage> functions = new Dictionary<string, MethodPackage>();

        public void addMethod(string methodName, MethodPackage methodPackage)
        {
            try
            {
                if (functions.ContainsKey(methodName))
                {
                    functions[methodName].Add(methodPackage);
                }
                else
                {
                    functions.Add(methodName, methodPackage);
                }
            }
            catch (Exception ex)
            {

            }
        }
        public void addDelegate(string methodName, string description, List<MethodPackage.Parameter> parameters, MethodPackage.Method deleg)
        {
            if (parameters == null) parameters = new List<MethodPackage.Parameter>();
            if (deleg == null)
            {
                deleg = delegate(string functionName, List<string> para) { return ""; };
            }
            if (String.IsNullOrWhiteSpace(description)) description = "";
            if (String.IsNullOrWhiteSpace(methodName)) methodName = "anonymous";
            addMethod(methodName, new MethodPackage(methodName, parameters, description, deleg));
        }

        protected List<MethodPackage.Parameter> getFreshParameterList()
        {
            return new List<MethodPackage.Parameter>();
        }

        
        abstract protected void setDelegates();


        #endregion

    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable()]
    [JsonConverter(typeof(PropertyNameMatchingConverter))]
    public class Matter : Commandable, JSerializable<ActionParser>, IComparable, ActionParser, Registrable<ActionParser>, Registrar<ActionParser>, DBActionable, StoryHelperLibrary.Interfaces.Mergable<Matter>
    {

        //protected readonly Regex regexIsVerb = new Regex(@"^(verb\(\-?(to be\s{1}|be\s{1}|to\s{1})?\w{1,}(\,|\;|\.|\:){1}\s*(0|1){0,1}\d{1}\)|v\(\-?(to be\s{1}|be\s{1}|to\s{1})?\w{1,}(\,|\;|\.|\:){1}\s*(0|1){0,1}\d{1}\))$", RegexOptions.IgnoreCase);
        //protected readonly Regex regexIsVerbWithAdverb = new Regex(@"^(verb\(\-?(to be\s{1}|be\s{1}|to\s{1})?\w{1,}(\,|\;|\.|\:){1}\s*(0|1){0,1}\d{1}(\,|\;|\.|\:){1}\s*\w{1,}\)|v\(\-?(to be\s{1}|be\s{1}|to\s{1})?\w{1,}(\,|\;|\.|\:){1}\s*(0|1){0,1}\d{1}(\,|\;|\.|\:){1}\s*\w{1,}\))$", RegexOptions.IgnoreCase);

        //public int tense = 1;

        public Matter() 
        {
            //setDelegates();
        }

        

        protected readonly Regex quickMonikers = new Regex(@"^(\@|\+|\%|\$|\$\$|\@\@|\+\+|\%\%){1}.*$");
        protected readonly Regex regexCallListWithKey = new Regex(@"^(A|S|F|V|SC|K)\(\w+(\.\w+)*\)$", RegexOptions.IgnoreCase);
        protected readonly Regex regexIsVerbToAdverb = new Regex(@"^verbToAdverb\(\w{1,}\)$", RegexOptions.IgnoreCase);
        protected readonly Regex regexIsAdverb = new Regex(@"^adverb\(\w+\)$", RegexOptions.IgnoreCase);
        protected readonly Regex regexIsNumberToText = new Regex(@"^numberToText\([0-9]+\)$", RegexOptions.IgnoreCase);

        protected const string postIndefinateArticleCode = "ART-AFTER-";
        protected const string multipleAdjectivesFinalizer = ", and ";
        protected const string justANDseparator = " and ";
        protected const string multipleAdjectivesSeperator = ", ";
        protected const string commandSeperator = ".";
        protected const string indefinateArticleCode = "ART";
        

        protected Random r = new Random();

        public bool colour_readonly { get; set; }
        public bool size_readonly { get; set; }
        public bool weight_readonly { get; set; }
        public bool name_readonly { get; set; }
        public bool other_readonly { get; set; }
        public bool isMany_readonly { get; set; }
        public bool adjectives_readonly { get; set; }
        public bool aliases_readonly { get; set; }
        public bool adverbs_readonly { get; set; }
        public bool states_readonly { get; set; }
        public bool who_readonly { get; set; }

        private bool isMany = false;
        public bool IsMany
        {
            get { return isMany; }
            set { isMany = value; }
        }
        
        public Pronouns pronouns = new Pronouns(Pronouns.Pronoun.It);

        public int dbID = -1;
        public int userId = -1;

        

        private bool isUsed = true;
        private int groupId = 0;
        public int GroupId
        {
            get { return groupId; }
            set { groupId = value; }
        }
        public bool IsUsed
        {
            get { return isUsed; }
            set { isUsed = value; }
        }

        private Registrar<ActionParser> owner = null;
        internal Registrar<ActionParser> Owner
        {
            get { return owner; }
            set { owner = value; }
        }

        private int globalPercentage = 100;
        public int GlobalPercentage
        {
            get { return globalPercentage; }
            set
            {
                if (value < 0) globalPercentage = 0;
                else if (value > 100) globalPercentage = 100;
                else globalPercentage = value;
            }
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

        public Matter(string name):this()
        {
            this.Name = name;

        }

        public Matter(string name, string id)
            : this(name)
        {
            this.Id = id;
        }

        private string other = "";

        public string Other
        {
            get { return other; }
            set { other = value; }
        }

        private string id = "";
        public string Id
        {
            get { return id.ToLower(); }
            set
            {
                if (!String.IsNullOrEmpty(value))
                {
                    if (value.Length < 40)
                        id = value.ToLower();
                    //this.addMatterToRegistry(this.id, this);
                }
            }
        }

        public string getId()
        {
            if (!String.IsNullOrEmpty(this.id))
                return this.id.ToLower();
            else
                return this.Name.ToLower();
        }

        public void setId(string id)
        {
            this.Id = id;
        }

        #region states
        private ListDrawString states = new ListDrawString(100);
        public List<string> States
        {
            get
            {
                this.states.replenish();
                return this.states;
            }
            set
            {

                states = new ListDrawString(statePercentage, value);
            }
        }
        public void addState(string state)
        {
            addStringToList(this.states, state);
        }
        public void addState(string[] states)
        {
            addStringsToList(this.states, states);
        }
        public string[] getStates()
        {
            return getList(this.states);
        }
        public int statePercentage = 100;

        public int StatePercentage
        {
            get
            {
                return statePercentage * this.globalPercentage / 100;
            }
            set
            {
                if (value < 0) statePercentage = 0;
                else if (value > 100) statePercentage = 100;
                else statePercentage = value;

                this.states.DrawRate = statePercentage * this.globalPercentage / 100;
            }
        }
        #endregion

        #region adverbs
        private ListDrawString adverbs = new ListDrawString(100);
        public List<string> Adverbs
        {
            get
            {
                //List<string> x = new List<string>();

                //foreach (string s in aliases)
                //{
                //    x.Add(s);
                //}
                //return x;
                this.adverbs.replenish();
                return this.adverbs;
            }
            set
            {

                adverbs = new ListDrawString(adverbsPercentage, value);
            }
        }
        public void addAdverb(string adverb)
        {
            addStringToList(this.adverbs, adverb);
        }
        public void addAdverb(string[] adverbs)
        {
            addStringsToList(this.adverbs, adverbs);
        }
        public string[] getAdverbs()
        {
            return getList(this.adverbs);
        }
        public int adverbsPercentage = 100;

        public int AdverbsPercentage
        {
            get
            {
                return adverbsPercentage * this.globalPercentage / 100;
            }
            set
            {
                if (value < 0) adverbsPercentage = 0;
                else if (value > 100) adverbsPercentage = 100;
                else adverbsPercentage = value;
                this.adverbs.DrawRate = adverbsPercentage * this.globalPercentage / 100;
            }
        }
        #endregion

        #region Who
        private ListDrawString who = new ListDrawString(100);
        public List<string> Who
        {
            get
            {
                //List<string> x = new List<string>();

                //foreach (string s in aliases)
                //{
                //    x.Add(s);
                //}
                //return x;
                this.who.replenish();
                return this.who;
            }
            set
            {

                adverbs = new ListDrawString(whoPercentage, value);
            }
        }
        public void addWho(string who)
        {
            addStringToList(this.who, who);
        }
        public void addWho(string[] who)
        {
            addStringsToList(this.who, who);
        }
        public string[] getWho()
        {
            return getList(this.who);
        }
        public int whoPercentage = 100;

        public int WhoPercentage
        {
            get
            {
                return whoPercentage * this.globalPercentage / 100;
            }
            set
            {
                if (value < 0) whoPercentage = 0;
                else if (value > 100) whoPercentage = 100;
                else whoPercentage = value;

                this.who.DrawRate = whoPercentage * this.globalPercentage / 100;
            }
        }
        #endregion

        #region alias
        private ListDrawString aliases = new ListDrawString(100);
        public List<string> Aliases
        {
            get
            {
                this.aliases.replenish();
                return this.aliases;
            }
            set
            {

                aliases = new ListDrawString(aliasPercentage, value);
            }
        }
        public void AddAlias(string alias)
        {
            addStringToList(this.aliases, alias);
        }
        public void AddAlias(string[] aliases)
        {
            addStringsToList(this.aliases, aliases);
        }
        public string[] getAliases()
        {
            return getList(this.aliases);
        }
        public int aliasPercentage = 100;

        public int AliasPercentage
        {
            get
            {
                return aliasPercentage * this.globalPercentage / 100;
            }
            set
            {
                if (value < 0) aliasPercentage = 0;
                else if (value > 100) aliasPercentage = 100;
                else aliasPercentage = value;

                this.aliases.DrawRate = aliasPercentage * this.globalPercentage / 100;
            }
        }
        #endregion

        #region adjectives
        private ListDrawString adjectives = new ListDrawString(100);
        public List<String> Adjectives
        {
            get
            {
                this.adjectives.replenish();
                return this.adjectives;
            }
            set { this.adjectives = new ListDrawString(adjectivesPercentage, value); }
        }
        public void AddAdjective(string adj)
        {
            addStringToList(this.adjectives, adj);
        }
        public void AddAdjective(string[] adjs)
        {
            addStringsToList(this.adjectives, adjs);
        }
        public string[] getAdjectives()
        {
            return getList(this.adjectives);
        }
        public int adjectivesPercentage = 100;

        public int AdjectivesPercentage
        {
            get
            {

                return adjectivesPercentage * this.globalPercentage / 100;
            }
            set
            {
                if (value < 0) adjectivesPercentage = 0;
                else if (value > 100) adjectivesPercentage = 100;
                else adjectivesPercentage = value;

                this.adjectives.DrawRate = adjectivesPercentage * this.globalPercentage / 100;
            }
        }
        #endregion

        private string name = "";

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                if (String.IsNullOrEmpty(value.Trim())) return;
                name = value.Replace("_", " ");
            }
        }

        private string colour = "";

        public string Colour
        {
            get
            {
                return colour;

            }
            set
            {
                //if (String.IsNullOrEmpty(value.Trim())) return;
                colour = value.Trim();
            }
        }




        private string size = "";
        public string Size
        {
            get
            {
                return size;
            }
            set
            {
                size = value;
            }
        }


        private int weight = 0;

        public int Weight
        {
            get
            {
                return this.weight;
            }
            set
            {
                this.weight = value;
            }
        }

        protected Dictionary<string, ActionParser> registry = new Dictionary<string, ActionParser>();
        private Dictionary<string, WordGroup> words = new Dictionary<string, WordGroup>();

        private Registrable<ActionParser> otherMatter = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        public void setOther(Registrable<ActionParser> other)
        {
            if (other != null)
                this.otherMatter = other;
        }

        public Registrable<ActionParser> getOther()
        {
            return this.otherMatter;
        }

        protected string[] getList(IList<string> list)
        {
            if (list is ListDrawString)
            {
                return (list as ListDrawString).getExpandedList().ToArray();
            }
            return list.ToArray();
            
        }

        protected void addStringToList(IList<string> list, string str)
        {
            if (String.IsNullOrEmpty(str.Trim())) return;


            // Let the ListDrawString Object do the validations
            if (list is ListDrawString) (list as ListDrawString).Add(str);
            else
            {
                if (!list.Contains(str)) list.Add(str);
            }

            //if (!list.Contains(str))
            //{
            //    if (list is ListDrawString) (list as ListDrawString).Add(str);
            //    else list.Add(str);
            //}
            //else if (str.Contains("=>"))
            //{
            //    if (list is ListDrawString) (list as ListDrawString).Add(str);
            //}
        }

        public void addStringsToList(IList<string> list, string[] strs)
        {
            if (list is ListDrawString) (list as ListDrawString).AddRange(strs);
            else
            {
                if (strs != null)
                {
                    foreach (string str in strs)
                    {

                        if (String.IsNullOrEmpty(str.Trim())) continue;
                        bool found = false;


                        foreach (string listString in list)
                        {
                            if (listString.Trim().ToLower().Equals(str.Trim().ToLower()))
                            {
                                found = true; break;
                            }
                        }
                        if (!found)
                        {
                            list.Add(str);
                        }
                    }
                }
            }
        }

        public string getOwnerId()
        {
            Registrar<ActionParser> owner = this.getOwner();
            if (owner == null) return this.getId();

            if (owner is Matter)
            {
                return (owner as Matter).getFullId();
            }

            return this.getId();
        }

        public string getOwnerName()
        {
            Registrar<ActionParser> owner = this.getOwner();

            if (owner is Matter && !owner.Equals(this))
            {
                return (owner as Matter).getOwnerName();
            }

            return this.name;
        }

        public void setPronoun(Pronouns.Pronoun pronoun)
        {
            this.pronouns = new Pronouns(pronoun);
        }

        public void setPronoun(string pronoun)
        {
            if (String.IsNullOrWhiteSpace(pronoun)) return;
            try
            {
                var pronounEnum = Pronouns.getPronounEnumFromString(pronoun);
                this.setPronoun(pronounEnum);
            }
            catch (Exception ex)
            {

            }
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("classType", this.GetType().Name);
            if (!String.IsNullOrEmpty(this.id)) info.AddValue("Id", this.Id);
            if (!String.IsNullOrEmpty(this.Name)) info.AddValue("Name", this.Name);
            if (!String.IsNullOrEmpty(this.Colour)) info.AddValue("Colour", this.Colour);
            if (!String.IsNullOrEmpty(this.Size)) info.AddValue("Size", this.Size);
            if (!String.IsNullOrEmpty(this.other)) info.AddValue("Other", this.other);
            if (this.Weight > 0) info.AddValue("Weight", this.Weight);
            if (this.adjectives.Count > 0) info.AddValue("Adjectives", this.Adjectives);
            if (this.aliases.Count > 0) info.AddValue("Aliases", this.Aliases);
            if (this.states.Count > 0) info.AddValue("States", this.States);
            if (this.adverbs.Count > 0) info.AddValue("Adverbs", this.Adverbs);
            if (this.who.Count > 0) info.AddValue("Who", this.Who);
            if (this.whoPercentage > 0) info.AddValue("WhoPercentage", this.whoPercentage);
            if (this.statePercentage > 0) info.AddValue("StatePercentage", this.statePercentage);
            if (this.adjectivesPercentage > 0) info.AddValue("AdjectivesPercentage", this.adjectivesPercentage);
            if (this.adverbsPercentage > 0) info.AddValue("AdverbsPercentage", this.adverbsPercentage);
            if (this.aliasPercentage > 0) info.AddValue("AliasPercentage", this.aliasPercentage);
            if (this.GlobalPercentage > 0) info.AddValue("GlobalPercentage", this.GlobalPercentage);
            info.AddValue("IsUsed", this.IsUsed);
            info.AddValue("GroupId", this.GroupId);
            info.AddValue("IsMany", this.isMany);
            info.AddValue("Pronoun", this.pronouns.selectedPronoun.ToString());
        }

        //Deserialization constructor.
        public Matter(SerializationInfo info, StreamingContext ctxt)
        {
            //Get the values from info and assign them to the appropriate properties
            try
            {
                this.Id = (string)info.GetValue("Id", typeof(string));
            }
            catch (Exception)
            { }
            try
            {
                this.Name = (string)info.GetValue("Name", typeof(string));
            }
            catch (Exception)
            { }
            try
            {
                this.Colour = (string)info.GetValue("Colour", typeof(string));
            }
            catch (Exception)
            { }
            try
            {
                this.Size = (string)info.GetValue("Size", typeof(string));
            }
            catch (Exception)
            { }
            try
            {
                this.other = (string)info.GetValue("Other", typeof(string));
            }
            catch (Exception)
            { }
            try
            {
                this.Weight = (int)info.GetValue("Weight", typeof(int));
            }
            catch (Exception)
            { }
            try
            {
                this.IsUsed = (bool)info.GetValue("IsUsed", typeof(bool));
            }
            catch (Exception)
            { }
            try
            {
                this.GroupId = (int)info.GetValue("GroupId", typeof(int));
            }
            catch (Exception)
            { }

            try
            {
                this.isMany = (bool)info.GetValue("IsMany", typeof(bool));
            }
            catch (Exception)
            { }
            try
            {

                List<string> x = (List<string>)info.GetValue("Adjectives", typeof(IList<string>));
                foreach (string str in x)
                {
                    this.AddAdjective(str);
                }

            }
            catch (Exception) { }
            try
            {
                List<string> x = (List<string>)info.GetValue("Aliases", typeof(IList<string>));
                foreach (string str in x)
                {
                    this.AddAlias(str);
                }

            }
            catch (Exception) { }
            try
            {
                List<string> x = (List<string>)info.GetValue("States", typeof(IList<string>));
                foreach (string str in x)
                {
                    this.addState(str);
                }

            }
            catch (Exception) { }
            try
            {
                List<string> x = (List<string>)info.GetValue("Adverbs", typeof(IList<string>));
                foreach (string str in x)
                {
                    this.addAdverb(str);
                }

            }
            catch (Exception) { }
            try
            {
                List<string> x = (List<string>)info.GetValue("Who", typeof(IList<string>));
                foreach (string str in x)
                {
                    this.addWho(str);
                }

            }
            catch (Exception) { }
            try
            {
                this.AliasPercentage = (int)info.GetValue("AliasPercentage", typeof(int));
            }
            catch (Exception)
            { }
            try
            {
                this.AdverbsPercentage = (int)info.GetValue("AdverbsPercentage", typeof(int));
            }
            catch (Exception)
            { }
            try
            {
                this.AdjectivesPercentage = (int)info.GetValue("AdjectivesPercentage", typeof(int));
            }
            catch (Exception)
            { }
            try
            {
                this.StatePercentage = (int)info.GetValue("StatePercentage", typeof(int));
            }
            catch (Exception)
            { }
            try
            {
                this.WhoPercentage = (int)info.GetValue("WhoPercentage", typeof(int));
            }
            catch (Exception)
            { }
            try
            {
                this.GlobalPercentage = (int)info.GetValue("GlobalPercentage", typeof(int));
            }
            catch (Exception)
            { }
            try
            {
                this.isMany_readonly = (bool)info.GetValue("isMany_readonly", typeof(bool));
            }
            catch (Exception)
            { }
            try
            {
                string pronoun = (string)info.GetValue("Pronoun", typeof(string));
                this.setPronoun(pronoun);
            }
            catch (Exception)
            {
            }
            //try
            //{
            //    Dictionary<string, WordGroup> x = (Dictionary<string, WordGroup>)info.GetValue("Aliases", typeof(IDictionary<string, WordGroup>));
            //    this.WordGroups = x;

            //}
            //catch (Exception) { }
        }

        //public override bool Equals(Object obj)
        //{
        //    Matter matterObj = obj as Matter;
        //    if (matterObj == null)
        //        return false;
        //    else
        //        return this.Id.Equals(matterObj.Id) && this.Name.Equals(matterObj.Name);
        //}

        //public bool setProperty<T>(Expression<Func<T>> property)
        //{
        //    var propertyInfo = ((MemberExpression)property.Body).Member as PropertyInfo;
        //    if (propertyInfo == null)
        //    {
        //        throw new ArgumentException("The lambda expression 'property' should point to a valid Property");
        //    }
        //}

        protected string drawMoi(string code, string failsafe, int tense)
        {
            ListDrawString listPrepositions = new ListDrawString(100, new string[] { "~the", "da" });
            if (!(this is Human)) listPrepositions.Add("wpa");
            ListDrawString listDescribers = new ListDrawString(100, new string[] { "as", "a", "sa" });
            ListDrawString listMonikers = new ListDrawString(100, new string[] { "k-a" });
            if (!(this is Human)) listMonikers.Add(",~of,wpp");
            List<string> moiList = new List<string>();
            moiList.Add(listPrepositions.draw());
            moiList.Add(listDescribers.draw());
            if (!moiList[0].Equals("wpa")) moiList.Add(listMonikers.draw());
            else moiList.Add("k");
            return this.refOther(code, failsafe, moiList.ToArray(), tense);
        }

        protected string drawMon(string code, string failsafe, int tense)
        {
            ListDrawString listPrepositions = new ListDrawString(100, new string[] { "wpa", "~the" });
            ListDrawString listDescribers = new ListDrawString(100, new string[] { "as", "a", "sa" });
            //ListDrawString listMonikers = new ListDrawString(100, new string[] { "k-a", "k-a.~of.wpp" });
            List<string> moiList = new List<string>();
            moiList.Add(listPrepositions.draw());
            moiList.Add(listDescribers.draw());
            if (!moiList[0].Equals("wpa")) moiList.Add("k.~of.wpp");
            else moiList.Add("k");
            return this.refOther(code, failsafe, moiList.ToArray(), tense);
        }

        /// <summary>
        /// Finds actor
        /// </summary>
        /// <param name="code">If the code is larger than 2 characters, for some reason the code will remove 2 characters.</param>
        /// <param name="failsafe">The failsafe code to send back to the user.</param>
        /// <param name="action">An array of actions the found actor will do.</param>
        /// <returns></returns>
        protected string refOther(string code, string failsafe, string[] action, int tense)
        {
            Regex regexIsParentheses = new Regex(@"^\(\w+\)$", RegexOptions.IgnoreCase);
            string referenceId = "";
            // no idea why
            if (code.Length > 2) code = code.Remove(0, 2);

            if (regexIsParentheses.IsMatch(code))
            {
                // remove the parentheses
                string actor = code.Remove(0, 1);
                referenceId = actor.Remove(actor.Length - 1, 1);

            }
            else
            {
                referenceId = "ref" + this.Id;
            }
            return referenceMeByOther(referenceId, failsafe, action, tense);
        }

        protected string referenceMeByOther(string referenceId, string failsafe, string[] action, int tense)
        {
            var o = this.getOther() as Human;
            if (o == null) return this.interpret(failsafe, tense);

            foreach (var x in o.registry.Where(y => y.Key.ToUpper().Trim() == referenceId.ToUpper().Trim()))
            {
                return x.Value.interpret(action, tense);
            }

            return this.interpret(failsafe, tense);
        }

        /// <summary>
        /// Draws from two ListDrawStrings, and then checks if the listFirst string is found in the listSecond string drawn. 
        /// If so, it trys again, to a maximum of 10 trys, after which, it returns what ever it last drawn. 
        /// </summary>
        /// <param name="listFirst"></param>
        /// <param name="listSecond"></param>
        /// <returns></returns>
        protected string drawNoConflict(ListDrawString listFirst, ListDrawString listSecond, int tense)
        {
            string adjective = this.interpret(listFirst.draw(), tense);
            string alias = this.interpret(listSecond.draw(), tense);
            bool isConflicting = true;
            int ctr = 0; // prevent infinite loops.

            while (ctr < 10 && isConflicting && listFirst.Count >= 1 && listSecond.Count >= 1)
            {
                adjective = this.interpret(listFirst.draw(), tense);
                if (!adjective.Trim().ToLower().Contains(alias.Trim().ToLower())) isConflicting = false;
                ctr++;
            }



            return adjective + " " + alias;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="picked"></param>
        /// <param name="list">The list of words to draw from</param>
        /// <param name="defaultAliasString"></param>
        /// <returns></returns>
        protected string drawNoConflict(string picked, ListDrawString list, int tense, string defaultAliasString = "")
        {
            //string defaultString = (this is Human) ? "TYPE" : "N";
            //return drawFromList(this.aliases, this.interpret(defaultString), this.AliasPercentage);

            string alias = defaultAliasString;
            bool isConflicting = true;
            int ctr = 0; // prevent infinite loops.
            string interpreted = "";

            // Add the failsafe to the list
            if (list.Count == 0) return picked + " " + alias;

            // If the list only has one possibility, which includes empty lists passed as parameter.
            if (list.Count == 1)
            {
                interpreted = this.interpret(picked, tense);
                alias = list.draw();
                // ensure that the list contains the preselected string code
                if (interpreted.Trim().ToLower().Contains(alias.Trim().ToLower())) return alias;
                return interpreted + " " + alias;
            }


            while (ctr < 10 && isConflicting)
            {
                alias = list.draw();
                interpreted = this.interpret(picked, tense);
                // ensure that the list contains the preselected string code
                if (!interpreted.Trim().ToLower().Contains(alias.Trim().ToLower())) isConflicting = false;
                ctr++;
            }

            // If more than 10 cycles occur, give up and send the failsafe by itself.
            if (ctr >= 10 && isConflicting) return alias;
            return picked + " " + alias;
        }

        protected string getIndefinateArticle(string code)
        {
            if (this.isMany) return "some " + code;

            foreach (string str in new string[] { "a", "e", "i", "o", "u" })
            {
                if (code.StartsWith(str)) return "an " + code;
            }
            return "a " + code;
        }

        protected string getIndefinateArticle(string alias, string adjectives)
        {
            string article = "a";
            if (String.IsNullOrEmpty(alias)) return "";

            if (!String.IsNullOrWhiteSpace(adjectives))
            {
                foreach (string str in new string[] { "a", "e", "i", "o", "u" })
                {
                    if (adjectives.StartsWith(str))  article = "an";
                }
            }
            else
            {

                foreach (string str in new string[] { "a", "e", "i", "o", "u" })
                {
                    if (alias.StartsWith(str)) article = "an";
                }
            }
            if (this.IsMany) article = "some";
            return article + " " + adjectives + " " + alias;
        }

        //protected string getAdverbAdjective(ListDrawString list)
        //{
        //    string theAdjective = this.drawFromList(list, "", this.AdjectivesPercentage);
        //    if (String.IsNullOrWhiteSpace(theAdjective)) return "";
        //    string theAdverb = list.getAssociated();
        //    return theAdverb + " " + theAdjective;
        //}

        protected string evaluateProbability(string word, int probability)
        {
            int value = r.Next(100);
            if (value > probability) return "";
            return word;
        }

        protected bool willDraw(int probability = 100)
        {
            int value = r.Next(100);
            if (value >= probability) return false;
            return true;
        }

        protected string drawWho()
        {
            return this.drawFromList(this.who, "");
        }

        protected string drawFromList(ListDrawString list, string defaultString, int probability = 100, bool linear = false)
        {
            //int value = r.Next(100);
            //if (value >= probability) return defaultString;

            if (list.ListCount() > 0)
            {

                if (!linear) return list.getAssociated(ListDrawString.DrawingStyle.Sublist);
                else
                {

                    string str = list.drawLinear();
                    // We rely on the fact that drawing from the list automatically sets LastSelected
                    string associated = list.getAssociated(ListDrawString.DrawingStyle.LastSublist);
                    // if there is no associated, return the key
                    if (String.IsNullOrWhiteSpace(associated))
                    {
                        return str;
                    }
                    else return associated;
                }
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
                int index = -1;
                index = str.IndexOf('.');
                if (index > -1)
                {
                    string[] cmds = str.Split(new char[] { '.' });
                    if (cmds.Length < 2)
                    {
                        returnValues[x++] = this.interpret(str, tense);
                        continue;
                    }
                    else
                    {
                        string gather = "";
                        foreach (var cmd in cmds)
                        {
                            gather += this.interpret(cmd, tense) + " ";
                        }
                        returnValues[x++] = gather.Trim();
                        continue;
                    }
                }
                returnValues[x++] = this.interpret(str, tense);
            }

            return String.Join(" ", returnValues);
        }

        protected string chainedAdjectives(int amount, string seperator, string ender = "")
        {

            return chainedDraws(amount, seperator, this.adjectives, ender);
        }

        protected string chainedDraws(int amount, string seperator, ListDrawString list, string ender = "")
        {
            string[] strs = list.draw(amount);
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

        protected List<string> copyList(IList<string> list)
        {
            return new List<string>(list.ToArray<string>());
        }

        public virtual ActionParser deepCopy()
        {
            Matter m = new Matter(this.name, this.id);
            m = copy(m);
            return m as ActionParser;
        }

        protected virtual Matter copy(Matter m)
        {
            Console.WriteLine(m.Name + " = Matter accessed");
            m.name = this.name;
            m.id = this.id;
            m.colour = this.colour;
            m.aliases = new ListDrawString(this.aliasPercentage, this.copyList(this.aliases));
            m.adjectives = new ListDrawString(this.adjectivesPercentage, this.copyList(this.adjectives));
            m.states = new ListDrawString(this.statePercentage, this.copyList(this.states));
            m.adverbs = new ListDrawString(this.adverbsPercentage, this.copyList(this.adverbs));
            m.who = new ListDrawString(this.whoPercentage, this.copyList(this.who));
            m.owner = this.owner;
            m.size = this.size;
            m.weight = this.weight;
            m.other = this.other;
            m.AliasPercentage = this.aliasPercentage;
            m.AdverbsPercentage = this.adverbsPercentage;
            m.AdjectivesPercentage = this.adjectivesPercentage;
            m.StatePercentage = this.statePercentage;
            m.WhoPercentage = this.whoPercentage;
            m.groupId = this.groupId;
            m.isUsed = this.isUsed;
            m.isMany = this.isMany;



            foreach (KeyValuePair<string, WordGroup> kp in this.words)
            {
                WordGroup w = new WordGroup(kp.Key);
                w.Words = this.copyList(kp.Value.Words);
                m.words.Add(kp.Key, w);
            }

            return m;
        }

        public string getName()
        {
            return this.Name;
        }
        #region NumberShit
        protected string getNumberInWordForm(int number)
        {

            char[] ageChar = number.ToString().ToCharArray();
            int[] age = new int[ageChar.Length];

            for (int x = 0; x < ageChar.Length; x++)
            {
                age[x] = int.Parse(ageChar[x].ToString());
            }

            switch (ageChar.Length)
            {
                case 0:
                    return "zero";
                case 1:
                    return this.getSingleDigit(number);
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

            return "zero";

        }

        private string getSingleDigit(int value)
        {

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

        private string getThreeDigit(int hundredth, int tenth, int first)
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

        #endregion

        #region LegacyVerbShit
        protected string getVerbOddityPresent()
        {
            if (!this.IsMany)
                return "s";
            return "";
        }

        protected string getVerbOddityPresentContinous()
        {
            if (!this.IsMany)
            {
                return "is";
            }
            else
            {
                return "are";
            }
        }

        protected string getVerbOddityPresentPerfect()
        {
            if (!this.IsMany)
            {
                return "has";
            }
            else
            {
                return "have";
            }

        }

        protected string getVerbOddityPresentPerfectContinuous()
        {
            if (!this.IsMany)
            {
                return "has";
            }
            else
            {
                return "have";
            }

        }

        protected string getVerbOddityPastContinuous()
        {
            if (!this.IsMany)
            {
                return "was";
            }
            else
            {
                return "were";
            }

        }
        #endregion

        #region Pronouns
        protected virtual string getPossesivePronoun()
        {
            return this.pronouns.Possessive;

            //if (!this.IsMany)
            //{
            //    return "its"; // not used

            //}
            //else
            //{
            //    return "theirs";
            //}


        }

        protected virtual string getSubjectPronoun()
        {
            return this.pronouns.Subject;

            //if (!this.IsMany)
            //{
            //    return "it";
            //}
            //else
            //{
            //    return "they";
            //}

        }

        protected virtual string getObjectPronouns()
        {
            return this.pronouns.Object;
            //if (!this.IsMany)
            //{
            //    return "it";
            //}
            //else
            //{
            //    return "them";
            //}
        }

        protected virtual string getPossessiveAdjectives()
        {
            return this.pronouns.Personal;
            //if (!this.IsMany)
            //{
            //    return "its";
            //}
            //else
            //{
            //    return "their";
            //}
        }

        protected virtual string getReflexivePronouns()
        {
            return this.pronouns.Reflexive;
            //if (!this.IsMany)
            //{
            //    return "itself";
            //}
            //else
            //{
            //    return "themselves";
            //}
        }
        #endregion

        public virtual ActionParser getClone()
        {
            DBMatter dbMatter = new DBMatter();
            dbMatter = this.getDBObejct(dbMatter) as DBMatter;
            Matter m = new Matter();
            m.setFromDBObject(dbMatter);
            return m;
        }

        public string getJson()
        {
            string muhString = Newtonsoft.Json.JsonConvert.SerializeObject(this);
            return muhString;
        }

        //public virtual ActionParser getClone()
        //{
        //    return this.deepCopy();
        //    //return (ActionParser)Newtonsoft.Json.JsonConvert.DeserializeObject<Matter>(this.getJson());
        //}

        protected void autoRegister(List<Registrable<ActionParser>> list)
        {
            foreach (Registrable<ActionParser> ap in list)
            {
                string name = ap.getName();
                string id = ap.getId();

                if (String.IsNullOrEmpty(id))
                {
                    ap.setId(name);
                    id = ap.getId();
                }

                try
                {
                    this.register(id, (ActionParser)ap, this.registry);
                }
                catch (Exception)
                {

                    this.registry.Remove(id);
                    try
                    {
                        this.register(id, (ActionParser)ap, this.registry);
                    }
                    catch (Exception)
                    {

                        Console.WriteLine("Unable to register " + id + " into the register of " + this.getName() + "'s registery.");
                    }

                    // already in the registry...
                    //throw;
                }
            }
        }

        public virtual void selfRegisterAll()
        {
            this.registry.Clear();
            // register words
            foreach (KeyValuePair<string, WordGroup> kp in this.words)
            {
                this.register(kp.Key, kp.Value, this.registry);
            }
        }
        
        public void analyseSettings(Settings settings)
        {
            this.startCommand = settings.startCommand;
            this.endCommand = settings.endCommand;
        }
        //====================================================================================================================================================================

        public void addToActionList(Registrable<ActionParser> a, List<Registrable<ActionParser>> list)
        {
            list.Add(a);
            if (!String.IsNullOrEmpty(((Registrable<ActionParser>)a).getId())) this.register(a.getId(), (ActionParser)a, this.getDictionaryOfRegisteredObjects());
        }

        public void addToActionList(List<Registrable<ActionParser>> listToAdd, List<Registrable<ActionParser>> list)
        {
            foreach (Registrable<ActionParser> a in listToAdd)
            {
                this.addToActionList(a, list);
            }
        }

        public ActionParser removeActionParser(int index, List<ActionParser> list)
        {
            ActionParser g = list[index];
            list.Remove(g);
            return g;
        }

        public bool isInRegistry<T>(string id, out T registeredObject) where T: ActionParser
        {
            registeredObject = (T)((Registrar<ActionParser>)this).getFromRegistry(id, this.registry);
            return (registeredObject == null) ? false : true;
        }

        public void register(string id, ActionParser item, Dictionary<string, ActionParser> registry)
        {
            if (!String.IsNullOrEmpty(id) && item != null)
            {
                try
                {
                    registry.Add(id, (ActionParser)item);
                    if (item is Registrable<ActionParser>)
                    {
                        (item as Registrable<ActionParser>).setOwner(this);
                    }
                }
                catch (Exception)
                {
                    try
                    {
                        this.registry.Remove(id);
                        registry.Add(id, (ActionParser)item);
                        if (item is Registrable<ActionParser>)
                        {
                            (item as Registrable<ActionParser>).setOwner(this);
                        }
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("A problem occured when registering " + item.getId() + ".");
                    }
                }
            }
        }

        ActionParser Registrar<ActionParser>.getFromRegistry(string id, Dictionary<string, ActionParser> registry)
        {
            ActionParser m = null;

            try
            {
                if (registry.Keys.Contains(id))
                {
                    m = (ActionParser)registry[id];
                }
                else
                {
                    if(m != null)
                    {
                        return m;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception)
            {
            }
            if (m != null)
            {
                return m;
            }

            return null;
        }

        public void register(List<ActionParser> items, Dictionary<string, ActionParser> registry)
        {
            foreach (ActionParser m in items)
            {
                this.register(m.getId(), m, registry);
            }
        }

        Dictionary<string, ActionParser> Registrar<ActionParser>.Registry
        {
            get
            {
                this.selfRegisterAll();
                return this.registry;
            }
            set
            {
                this.registry = value;
            }
        }

        public Dictionary<string, ActionParser> getDictionaryOfRegisteredObjects()
        {
            this.selfRegisterAll();
            return this.registry;
        }

        public ActionParser find(string id)
        {
            var dic = this.getDictionaryOfRegisteredObjects();
            ActionParser foundName = null;

            try
            {
                if (dic[id].getId().ToLower() == id.ToLower())
                {
                    //worked
                    foundName = dic[id];
                    return foundName;
                }
            }
            catch (Exception)
            {
                
            }
            return null;
        }

        public static T ObjectFromJson<T>(string json) where T : Matter
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

        public string getFullId()
        {
            string id = "";
            if (this.owner != null && this.owner is Matter)
            {
                id = (this.owner as Matter).getFullId() + ".";
            }

            return (id + this.getId()).ToLower();
        }

        public override string ToString()
        {
            if (this.Name.Trim().ToLower().Equals(this.getId().Trim().ToLower()))
                return this.Name;
            return this.Name + " (" + this.getId() + ")";
        }

        public virtual DBbase getDBObejct(DBbase o)
        {
            DBMatter d = o as DBMatter;
            if (d == null) d = new DBMatter();

            d.id = this.dbID;
            //d.scriptId = this.getId();
            d.userId = this.userId;
            d.name = this.getName();
            d.pronoun = this.pronouns.selectedPronoun.ToString();
            d.adjectives = String.Join("|", this.adjectives.getExpandedList().ToArray());
            d.aliases = String.Join("|", this.aliases.getExpandedList().ToArray());
            d.states = String.Join("|", this.states.getExpandedList().ToArray());
            d.adverbs = String.Join("|", this.adverbs.getExpandedList().ToArray());
            d.who = String.Join("|", this.who.getExpandedList().ToArray());
            d.colour = this.colour;
            d.size = this.size;
            d.weight = this.weight;
            d.other = this.other;
            d.statesPercentage = this.statePercentage;
            d.whoPercentage = this.whoPercentage;
            d.adverbsPercentage = this.adverbsPercentage;
            d.adjectivesPercentage = this.adjectivesPercentage;
            d.aliasPercentage = this.aliasPercentage;
            d.globalPercentage = this.GlobalPercentage;
            d.groupId = this.groupId;
            //d.isMany = this.isMany;
            d.isMany = this.isMany ? 1 : 0;
            d.isUsed = this.isUsed ? 1 : 0;
            d.adjectives_readonly = this.adjectives_readonly ? 1 : 0;
            d.adverbs_readonly = this.adverbs_readonly ? 1 : 0;
            d.aliases_readonly = this.aliases_readonly ? 1 : 0;
            d.colour_readonly = this.colour_readonly ? 1 : 0;
            d.isMany_readonly = this.isMany_readonly ? 1 : 0;
            //d.name_readonly = this.name_readonly ? 1 : 0;
            d.other_readonly = this.other_readonly ? 1 : 0;
            d.size_readonly = this.size_readonly ? 1 : 0;
            d.states_readonly = this.states_readonly ? 1 : 0;
            d.weight_readonly = this.weight_readonly ? 1 : 0;
            d.who_readonly = this.who_readonly ? 1 : 0;

            return d as DBbase;
        }

        public virtual void setFromDBObject(DBbase o)
        {
            DBMatter d = o as DBMatter;
            if (d == null) d = new DBMatter();



            this.setPronoun(d.pronoun);
            this.weight = d.weight;
            this.size = d.size;
            this.name = d.name;
            //this.id = d.scriptId;
            this.colour = d.colour;
            this.other = d.other;
            this.statePercentage = d.statesPercentage;
            this.whoPercentage = d.whoPercentage;
            this.adverbsPercentage = d.adverbsPercentage;
            this.adjectivesPercentage = d.adjectivesPercentage;
            this.aliasPercentage = d.aliasPercentage;
            this.GlobalPercentage = d.globalPercentage;
            this.groupId = d.groupId;
            this.dbID = d.id;
            this.userId = d.userId;

            this.isMany = d.isMany == 1 ? true : false;
            this.isUsed = d.isUsed == 1 ? true : false;
            this.adjectives_readonly = d.adjectives_readonly == 1 ? true : false;
            this.adverbs_readonly = d.adverbs_readonly == 1 ? true : false;
            this.aliases_readonly = d.aliases_readonly == 1 ? true : false;
            this.colour_readonly = d.colour_readonly == 1 ? true : false;
            this.isMany_readonly = d.isMany_readonly == 1 ? true : false;
            //this.name_readonly = d.name_readonly == 1 ? true : false;
            this.other_readonly = d.other_readonly == 1 ? true : false;
            this.size_readonly = d.size_readonly == 1 ? true : false;
            this.states_readonly = d.states_readonly == 1 ? true : false;
            this.weight_readonly = d.weight_readonly == 1 ? true : false;
            this.who_readonly = d.who_readonly == 1 ? true : false;

            ListDrawString.SetStringListFromCSV(this.aliases, d.aliases);
            ListDrawString.SetStringListFromCSV(this.adjectives, d.adjectives);
            ListDrawString.SetStringListFromCSV(this.states, d.states);
            ListDrawString.SetStringListFromCSV(this.adverbs, d.adverbs);
            ListDrawString.SetStringListFromCSV(this.who, d.who);
        }

        

        public override bool Equals(System.Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            Matter p = obj as Matter;
            if ((System.Object)p == null)
            {
                return false;
            }

            // check for reference equality.
            if (System.Object.ReferenceEquals(this, p)) return true;

            // Return true if the fields match:

            return
                (this.getFullId().Equals(p.getFullId()));// &&
                //(this.name.Equals(p.name));
        }

        public virtual int CompareTo(object obj)
        {
            if (obj == null)
                return 1;
            if (!(obj is Matter))
                return -1;
            if ((obj as Matter).Equals(this))
                return 0;
            return this.getId().CompareTo(((Matter)obj).getId());
        }

        public void setId(long id)
        {
            this.dbID = (int)id;
        }

        public enum ListCode
        {
            Adjective,
            State,
            Adverb,
            AdjectiveAndState,
            AdjectiveAndAdverb,
            StateAndAdverb,
            AdjectiveStateAdverb
        }

        protected ListDrawString getDescribingList(string code)
        {
            switch (code.ToUpper())
            {
                case "M":
                    return getDescribingList(ListCode.AdjectiveAndState);
                case "A":
                    return getDescribingList(ListCode.Adjective);
                case "V":
                    return getDescribingList(ListCode.Adverb);
                case "S":
                    return getDescribingList(ListCode.State);
                case "AS":
                    return getDescribingList(ListCode.AdjectiveAndState);
                case "SA":
                    return getDescribingList(ListCode.AdjectiveAndState);
                case "SAV":
                    return getDescribingList(ListCode.AdjectiveStateAdverb);
                case "VS":
                    return getDescribingList(ListCode.StateAndAdverb);
                case "SV":
                    return getDescribingList(ListCode.StateAndAdverb);
                case "AV":
                    return getDescribingList(ListCode.AdjectiveAndAdverb);
                case "VA":
                    return getDescribingList(ListCode.AdjectiveAndAdverb);
            }

            return getDescribingList(ListCode.Adjective);
        }

        protected ListDrawString getDescribingList(ListCode listCode)
        {
            ListDrawString list = new ListDrawString(100);
            switch (listCode)
            {
                case ListCode.Adjective:
                    this.addStringsToList(list, this.Adjectives.ToArray());
                    return list;
                case ListCode.AdjectiveAndAdverb:
                    this.addStringsToList(list, this.Adjectives.ToArray());
                    this.addStringsToList(list, this.Adverbs.ToArray());
                    return list;
                case ListCode.AdjectiveAndState:
                    this.addStringsToList(list, this.Adjectives.ToArray());
                    this.addStringsToList(list, this.States.ToArray());
                    return list;
                case ListCode.AdjectiveStateAdverb:
                    this.addStringsToList(list, this.Adjectives.ToArray());
                    this.addStringsToList(list, this.States.ToArray());
                    this.addStringsToList(list, this.Adverbs.ToArray());
                    return list;
                case ListCode.Adverb:
                    this.addStringsToList(list, this.Adverbs.ToArray());
                    return list;
                case ListCode.State:
                    this.addStringsToList(list, this.States.ToArray());
                    return list;
                case ListCode.StateAndAdverb:
                    this.addStringsToList(list, this.States.ToArray());
                    this.addStringsToList(list, this.Adverbs.ToArray());
                    return list;


            }

            return list;
        }

        // must have started with $,@,%, or +. Those symbols must have been parsed out before as well. 
        protected string multiAdjectiveNounPhrase(string code, string failsafeCode, string article, int tense, ListDrawString list)
        {
            if (String.IsNullOrWhiteSpace(code)) return this.getSubjectPronoun();
            while(code.Length > 0 && (
                code[0] == '$' ||
                code[0] == '@' ||
                code[0] == '%' ||
                code[0] == '+'
                    ) 
                )
            {
                code = code.Remove(0, 1);
                
            }

            int hasStateCode = code.ToUpper().IndexOf('S');
            int hasMixCode = code.ToUpper().IndexOf('M');

            if (hasStateCode > -1)
            {
                code = code.Replace("s", "").Replace("S", "");
            }
            if (hasMixCode > -1)
            {
                code = code.Replace("m", "").Replace("M", "");
            }


            ListDrawString mixedList = new ListDrawString(100);
            if (hasMixCode > -1)
            {
                this.addStringsToList(mixedList, this.Adjectives.ToArray());
                this.addStringsToList(mixedList, this.States.ToArray());
            }
            else
            {
                if (hasStateCode > -1)
                {
                    mixedList = this.states;
                }
                else
                {
                    mixedList = this.adjectives;
                }
            }

            if (hasStateCode > -1)
            {
                code = code.Replace("m", "").Replace("M", "");
            }

            int number = -1;
            string adjectives = "";
            if (int.TryParse(code, out number))
            {
                if (number > 0)
                {
                    adjectives = this.chainedDraws(number, Matter.multipleAdjectivesSeperator, mixedList, Matter.multipleAdjectivesFinalizer);
                }
            }


            // no idea what this code is - not a number 
            if (String.IsNullOrWhiteSpace(adjectives))
            {
                return this.interpret(failsafeCode, tense);
            }

            bool isHuman = (this is Human);
            string interpretedFailsafe;

            if (isHuman)
            {
                interpretedFailsafe = this.interpret("TYPE", tense);
            }
            else
            {
                interpretedFailsafe = this.interpret("N", tense);
            }


            string chosen = this.drawNoConflict(adjectives, list, tense, interpretedFailsafe);
            return article + " " + chosen;
        }

        

        public string getArticle(string code, int tense)
        {
            if (code.ToUpper().StartsWith(postIndefinateArticleCode))
            {
                code = code.Remove(0, 10);
                return this.getIndefinateArticle(code);
            }

            if (code.ToUpper().Equals(indefinateArticleCode))
            {
                string adjectivesAsString = this.interpret(this.chainedDraws(1, multipleAdjectivesSeperator, this.states, multipleAdjectivesFinalizer), tense);
                string selectedMoniker = this.interpret("k", tense);
                return this.startCommand + this.getFullId() + "." + postIndefinateArticleCode + adjectivesAsString + " " + selectedMoniker + this.endCommand;
            }


            if (code.ToUpper().StartsWith(indefinateArticleCode))
            {
                if (code.Length > indefinateArticleCode.Length)
                {
                    code = code.Remove(0, indefinateArticleCode.Length);
                    if ((this is Human) && (this as Human).Perspective != 3) return this.getSubjectPronoun();
                    return this.startCommand + this.getFullId() + "." + postIndefinateArticleCode + multiAdjectiveNounPhrase(code, "$", "", tense, this.aliases).Trim() + this.endCommand;


                    //code = code.Remove(0, indefinateArticleCode.Length);
                    //return this.interpret("*" + code);
                }
                else
                {
                    if (willDraw(this.AliasPercentage))
                    {

                        string adjectivesAsString = this.interpret(this.chainedDraws(1, multipleAdjectivesSeperator, this.states, multipleAdjectivesFinalizer), tense);
                        string selectedMoniker = this.interpret("k", tense);
                        return this.startCommand + this.getFullId() + "." + postIndefinateArticleCode + adjectivesAsString + " " + selectedMoniker + this.endCommand;

                        //string output = "the " + this.interpret("yar");
                        //if (output.Length > 4) return output; else return this.interpret("sp");
                    }
                    else
                    {
                        if (this is Human) return this.interpret("sp", tense);
                        return "the " + this.interpret("k", tense);
                    }
                }
            }

            return "";
        }

        

        //protected string methodParse(string command, out int start, out int end, string methodToken, string characterParameterStarter, string characterParameterEnder, int startingPoint = 0)
        //{
        //    start = 0; end = 0;

        //    start = command.IndexOf(methodToken, startingPoint);
        //    if (start == -1) return command;
        //    if (characterParameterStarter == characterParameterEnder) return command;

        //    int carriageLocation = start;

        //    int nestedAmount = 0;
        //    while (carriageLocation < command.Length - methodToken.Length && (command.Substring(++carriageLocation, methodToken.Length) != methodToken || nestedAmount > 0))
        //    {
        //        if (carriageLocation < command.Length - characterParameterStarter.Length && command.Substring(carriageLocation, characterParameterStarter.Length) == characterParameterStarter) nestedAmount++;
        //        if (carriageLocation < command.Length - characterParameterEnder.Length && command.Substring(carriageLocation, characterParameterEnder.Length) == characterParameterEnder) nestedAmount--;
        //    }

        //    if (nestedAmount != 0) return command;
        //    end = carriageLocation;
        //    if (carriageLocation - (start + methodToken.Length) < 0) return command.Substring(start + methodToken.Length);
        //    return command.Substring(start + methodToken.Length, carriageLocation - (start + methodToken.Length));
        //}

        //protected List<string> parseCommandMethods(string command, string methodToken, string characterParameterStarter, string characterParameterEnder, int startingPoint = 0)
        //{
        //    int start = 0; int end = 0;
        //    List<string> list = new List<string>();

        //    start = command.IndexOf(methodToken, startingPoint);
        //    if (start == -1 || characterParameterStarter == characterParameterEnder)
        //    {
        //        list.Add(command);
        //        return list;
        //    }

        //    list.Add(command.Substring(0, start));

        //    int carriageLocation = start;

        //    while (carriageLocation < command.Length - methodToken.Length)
        //    {
        //        string method = methodParse(command, out start, out end, methodToken, characterParameterStarter, characterParameterEnder, carriageLocation);
        //        if (!String.IsNullOrWhiteSpace(method)) list.Add(method);
        //        carriageLocation = end;
        //    }

        //    return list;
        //}

        #region Method Delegate Stuff
        //public class MethodPackage
        //{
        //    public string MethodName { get; set; }
        //    public string Description { get; set; }
        //    public List<Parameter> ParameterDescriptions = new List<Parameter>();
        //    public delegate string Method(string methodName, List<string> parameters);
        //    public Dictionary<string, Method> methods = new Dictionary<string, Method>();
        //    public void Add(MethodPackage package)
        //    {
        //        // overridding behavior.
        //        for (int x = package.methods.Count - 1; x > -1; x--)
        //        {
        //            bool found = false;
        //            for (int y = this.methods.Count - 1; y > -1; y--)
        //            {
        //                if (package.methods.ElementAt(x).Key.Equals(this.methods.ElementAt(y).Key))
        //                {
        //                    this.methods[this.methods.ElementAt(y).Key] = package.methods.ElementAt(x).Value;
        //                    found = true;
        //                    break;
        //                }
        //            }
        //            if(!found) this.methods.Add(package.methods.ElementAt(x).Key, package.methods.ElementAt(x).Value);
        //        }
                
        //    }
        //    public MethodPackage(string methodName, List<Parameter> parameterDescriptions, string description, Method method)
        //    {
        //        this.MethodName = methodName;
        //        this.Description = description;
        //        this.ParameterDescriptions = parameterDescriptions;
        //        this.methods.Add(this.getSignature(parameterDescriptions), method);
        //    }
        //    private string getSignature(List<Parameter> parameterDescriptions)
        //    {
        //        string final = this.MethodName;
        //        if (parameterDescriptions == null) return final;
        //        foreach (var p in parameterDescriptions)
        //        {
        //            final += "|" + p.ToString();
        //        }
        //        return final;
        //    }
        //    private string getSignature(List<string> parameters)
        //    {
        //        string final = this.MethodName;
        //        if (parameters == null) return final;
        //        foreach (var p in parameters)
        //        {
        //            final += "|" + Parameter.getParameterTypeName(p);
        //        }
        //        return final;
        //    }
        //    private string getSignature()
        //    {
        //        return this.getSignature(this.ParameterDescriptions);
        //    }
        //    public string Invoke(List<string> parameters)
        //    {
        //        try
        //        {

        //            string key = getSignature(parameters);
        //            if (methods.ContainsKey(key))
        //            {
        //                return methods[key].Invoke(this.MethodName, parameters);
        //            }
        //            return "Error: No method " + key;
        //        }
        //        catch (Exception ex)
        //        {
        //            throw ex;
        //        }
        //    }
        //    public override bool Equals(object obj)
        //    {
        //        if (obj == null) return false;
        //        if(!obj.GetType().Equals(typeof(MethodPackage))) return false;
        //        return (obj as MethodPackage).getSignature().Equals(this.getSignature());
        //    }

        //    public class Parameter : IComparable 
        //    {
        //        public string Name { get; set; }
        //        public string Description { get; set; }
        //        private Type ParameterType { get; set; }
        //        private object Value { get; set; }
        //        public Parameter(string name, string description, object value)
        //        {
        //            this.Name = name;
        //            this.Description = description;
        //            if (value == null) throw new Exception("Parameter value cannot be null.");
        //            this.ParameterType = value.GetType();
        //            this.Value = value;
        //        }

        //        private Parameter(string name, string description)
        //        {
        //            this.Name = name;
        //            this.Description = description;
        //        }

        //        public Parameter(string name, string description, Type type): this(name, description)
        //        {
        //            if (type.Equals(typeof(string)) || type.Equals(typeof(int)) || type.Equals(typeof(double)))
        //            {
        //                this.ParameterType = type;
        //            }
        //            else
        //            {
        //                throw new Exception("Parameter type is not allowed.");
        //            }
        //        }

        //        public Parameter(string name, string description, string value):this(name, description)
        //        {
        //            this.setValue(value);
        //        }

        //        public static Parameter getParameter(string value)
        //        {
        //            Parameter p = new Parameter("", "", value);
        //            return p;
        //        }

        //        public static Type parseParameter(string parameter, out object parsed)
        //        {
        //            parsed = null;
        //            if (String.IsNullOrWhiteSpace(parameter))
        //            {
        //                parsed = parameter;
        //                return typeof(string);
        //            }
        //            int number = -1;
        //            if (int.TryParse(parameter, out number))
        //            {
        //                parsed = number;
        //                return typeof(int);
        //            }

        //            double number2 = -1;
        //            if (double.TryParse(parameter, out number2))
        //            {
        //                parsed = number2;
        //                return typeof(double);
        //            }

        //            parsed = parameter;
        //            return typeof(string);
                    
        //        }

        //        public static Type getParameterType(string value)
        //        {
        //            if (String.IsNullOrWhiteSpace(value))
        //            {
        //                return typeof(string);
        //            }
        //            int number = -1;
        //            if (int.TryParse(value, out number))
        //            {
        //                return typeof(int);
        //            }

        //            double number2 = -1;
        //            if (double.TryParse(value, out number2))
        //            {
        //                return typeof(double);
        //            }

        //            return typeof(string);
        //        }

        //        public static string getParameterTypeName(string value)
        //        {
        //            if (String.IsNullOrWhiteSpace(value))
        //            {
        //                return "string";
        //            }
        //            int number = -1;
        //            if (int.TryParse(value, out number))
        //            {
        //                return "int";
        //            }

        //            double number2 = -1;
        //            if (double.TryParse(value, out number2))
        //            {
        //                return "double";
        //            }

        //            return "string";
        //        }

        //        public void setValue(string value)
        //        {
        //            if (String.IsNullOrWhiteSpace(value))
        //            {
        //                this.Value = "";
        //                this.ParameterType = typeof(string);
        //                return;
        //            }
        //            int number = -1;
        //            if (int.TryParse(value, out number))
        //            {
        //                this.ParameterType = typeof(int);
        //                this.Value = number;
        //                return;
        //            }

        //            double number2 = -1;
        //            if (double.TryParse(value, out number2))
        //            {
        //                this.ParameterType = typeof(double);
        //                this.Value = number2;
        //                return;
        //            }

        //            this.ParameterType = typeof(string);
        //            this.Value = value;
        //        }

        //        //public object getValue()
        //        //{
        //        //    if (this.Value.GetType().Equals(this.ParameterType))
        //        //    {
        //        //        return Value;
        //        //    }
        //        //    else throw new Exception("Incorrect Type");
        //        //}

        //        public dynamic getValue()
        //        {
        //            return Convert.ChangeType(this.Value, this.ParameterType);
        //        }

        //        public int CompareTo(object obj)
        //        {
        //            if (obj == null)
        //                return 1;
        //            if (!(obj is Parameter))
        //                return -1;
        //            if ((obj as Parameter).Equals(this))
        //                return 0;
        //            return this.Name.CompareTo(((Parameter)obj).Name);
        //        }

        //        public override bool Equals(object obj)
        //        {
        //            if(obj.GetType().Equals(this.GetType()))
        //            {
        //                return (obj as Parameter).ParameterType.Equals(this.ParameterType);
        //            }
        //            if (this.ParameterType.Equals(typeof(string)))
        //            {
        //                return true;
        //            }
        //            return obj.GetType().Equals(this.ParameterType);

        //            //if(!isSame) return false;
        //            //isSame &= (obj as Parameter).Name.Equals(this.Name);
        //            //isSame &= (obj as Parameter).ParameterType.Equals(this.ParameterType);

        //        }

        //        public override string ToString()
        //        {
        //            if(this.ParameterType.Equals(typeof(string)))
        //            {
        //                return "string";
        //            }
        //            if (this.ParameterType.Equals(typeof(int)))
        //            {
        //                return "int";
        //            }
        //            if (this.ParameterType.Equals(typeof(double)))
        //            {
        //                return "double";
        //            }

        //            return "string";
        //        }
        //    }
        //}

        //public Dictionary<string, MethodPackage> functions = new Dictionary<string, MethodPackage>();

        //public void addMethod(string methodName, MethodPackage methodPackage)
        //{
        //    try
        //    {
        //        if (functions.ContainsKey(methodName))
        //        {
        //            functions[methodName].Add(methodPackage);
        //        }
        //        else
        //        {
        //            functions.Add(methodName, methodPackage);
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}
        //public void addDelegate(string methodName, string description, List<MethodPackage.Parameter> parameters, MethodPackage.Method deleg)
        //{
        //    if (parameters == null) parameters = new List<MethodPackage.Parameter>();
        //    if (deleg == null)
        //    {
        //        deleg = delegate(string functionName, List<string> para) { return ""; };
        //    }
        //    if (String.IsNullOrWhiteSpace(description)) description = "";
        //    if (String.IsNullOrWhiteSpace(methodName)) methodName = "anonymous";
        //    addMethod(methodName, new MethodPackage(methodName, parameters, description, deleg));
        //}

        //protected List<MethodPackage.Parameter> getFreshParameterDictionary()
        //{
        //    return new List<MethodPackage.Parameter>();
        //}

        //protected int tense = 2;
        protected override void setDelegates()
        {
            MethodPackage.Method method = delegate(string methodName, List<string> parameters)
            {
                return "";
            };
            var parametres = getFreshParameterList();
            string description = "";


            addDelegate("N", "Returns the Name", null, (x, y) => { return this.Name; });
            addDelegate("ID", "Returns this object's ID", null, (x, y) => { return this.getFullId(); });
            addDelegate("C", "Returns the Size", null, (x, y) => { return this.Colour; });
            addDelegate("Z", "Returns the Size", null, (x, y) => { return this.Size; });
            description = "Draws multiple adjectives.";
            addDelegate("A5", description, null, (x, y) => { return this.chainedAdjectives(5, justANDseparator); });
            addDelegate("A2", description, null, (x, y) => { return this.chainedAdjectives(2, justANDseparator); });
            addDelegate("A3", description, null, (x, y) => { return this.chainedAdjectives(3, justANDseparator); });
            addDelegate("A4", description, null, (x, y) => { return this.chainedAdjectives(4, justANDseparator); });
            addDelegate("S2", "", null, (x, y) => { return this.chainedDraws(2, justANDseparator, this.states, Matter.multipleAdjectivesFinalizer); });
            addDelegate("SA", "Returns a state if there is one. If not, it returns an adjective, else an empty string.", null, (x, y) =>
            {
                string state = this.drawFromList(this.states, "", this.StatePercentage);
                string adj = this.chainedAdjectives(1, "");
                if (String.IsNullOrWhiteSpace(state))
                {
                    if (String.IsNullOrWhiteSpace(adj)) return "";
                    return adj;
                }
                if (String.IsNullOrWhiteSpace(adj)) return state;
                return state + Matter.justANDseparator + this.chainedAdjectives(1, "");
            });
            addDelegate("AS", "Either a state or adjective is returned. The adjective has a 70% chance over state. If one is empty, the other is returned.", null, (x, y) =>
            {
                string state1 = this.drawFromList(this.states, "", this.StatePercentage);
                string adjective1 = this.chainedAdjectives(1, "");
                if (String.IsNullOrWhiteSpace(state1)) return adjective1;
                if (String.IsNullOrWhiteSpace(adjective1)) return state1;
                int ran = this.r.Next(0, 10);
                return ((ran < 7) ? adjective1 : state1);
            });
            addDelegate("MOI", "Depending on what is set as 'other', this command returns a determiner phrase that describes the current object how they see it. MOI requires that other has a Trait which follows the following syntax for its script ID: ref", null, (x, y) => { return drawMoi("XX", "$", tense); });
            addDelegate("TOI", "Depending on what is set as 'other', this command returns a determiner phrase that describes the current object how they see it. TOI requires that other has a Trait which follows the following syntax for its script ID: ref", null, (x, y) => { return drawMoi("XX", "@", tense); });
            addDelegate("MON", "Depending on what is set as 'other', this command returns a possessive determiner phrase that describes the current object how they see it and own it. MON requires that other has a Trait which follows the following syntax for its script ID: ref", null, (x, y) => { return drawMon("XX", "+", tense); });
            addDelegate("Y", "Draws 'no conflict' from bother adjectives and monikers.", null, (x, y) => { string eitherOr = (this.r.Next(0, 100) > 50) ? "A-A" : "S-A"; return this.drawNoConflict(this.interpret(eitherOr, tense), this.aliases, tense, this.aliasFailSafe()); });
            addDelegate("YAR", "Does a command 'A-A' and draws from the moniker list. If no moniker exists, the command substitutes that with the 'TYPE' command for Humans, and 'N' command for objects.", null,
                (x, y) =>
                {
                    return this.drawNoConflict(this.doDrawFromListString("ADJECTIVES", "A"), this.aliases, tense, this.aliasFailSafe()); ;
                    return this.drawNoConflict(this.interpret("A-A", tense), this.aliases, tense, this.aliasFailSafe());
                });
            addDelegate("YSR", "Does a command 'S-A' and draws from the moniker list. If no moniker exists, the command substitutes that with the 'TYPE' command for Humans, and 'N' command for objects.", null,
                (x, y) =>
                {
                    return this.drawNoConflict(this.doDrawFromListString("STATES", "A"), this.aliases, tense, this.aliasFailSafe());
                    return this.drawNoConflict(this.interpret("S-A", tense), this.aliases, tense, this.aliasFailSafe());
                });
            addDelegate("W", "Gets the weight in digit form.", null, (x, y) => { return this.Weight.ToString(); });
            addDelegate("W-W", "Gets the weight in word form.", null, (x, y) => { return this.getNumberInWordForm(this.Weight); });
            addDelegate("W-WH", "Gets the weight in word form with hyphens.", null, (x, y) => { return this.getNumberInWordForm(this.Weight).Replace(' ', '-'); });
            addDelegate("OTHER", "Returns the script ID of the object specified in the 'other' textbox.", null, (x, y) => { return this.other; });
            addDelegate("F-C", "Returns the list item with surrounding commas. Intended to act like a mid-sentence preposition. e.g. [man.$.f-c.verb(attacked)]", null, (x, y) => { return evaluateProbability(", " + this.who.getAssociated() + ", ", this.WhoPercentage); });
            
            description = "Draws from list a random string.";
            addDelegate("F", description, null, (x, y) => { return this.who.draw(); });
            addDelegate("S", description, null, (x, y) => { return this.states.draw(); });
            addDelegate("V", description, null, (x, y) => { return this.adverbs.draw(); });
            addDelegate("A", description, null, (x, y) => { return this.adjectives.draw(); });
            addDelegate("K", description, null, (x, y) => { string monikerFound = this.aliases.draw(); if (String.IsNullOrWhiteSpace(monikerFound)) return this.aliasFailSafe(); return monikerFound; });
            
            description = "Draws from the list defined by the list chosen in a way defined by operation.";
            parametres = getFreshParameterList();
            parametres.Add(new MethodPackage.Parameter("Array of Strings", "The main list to draw from.", typeof(string)));
            parametres.Add(new MethodPackage.Parameter("Operation", "The Operation to be executed on the list.\nLinear,Last,Key,Value,Forced,Both", typeof(string)));
            addDelegate("DRAW", description, parametres, (x, y) => {return this.doDrawFromListString(MethodPackage.Parameter.getParameter(y[0]).getValue(), MethodPackage.Parameter.getParameter(y[1]).getValue());});

            description = "Noun Phrase. Draws from the alias list defined by the list chosen in a way defined by operation parameter";
            parametres = getFreshParameterList();
            parametres.Add(new MethodPackage.Parameter("Operation", "The Operation to be executed on the list.\nLinear,Last,Key,Value,Forced,Both", typeof(string)));
            parametres.Add(new MethodPackage.Parameter("Count", "The Amount to draw from a describing list, either Adj, Adv, states.", typeof(int)));
            parametres.Add(new MethodPackage.Parameter("Mode", "Certain modes behave differently. M,A,V,S,AS,SA,VA,AV,VS,SV,SAV", typeof(string)));
            parametres.Add(new MethodPackage.Parameter("Article", "Indefinet article code to use., This,That,The,A,My,None", typeof(string)));
            addDelegate("NP", description, parametres, (x, y) => 
            {
                string param1 = MethodPackage.Parameter.getParameter(y[0]).getValue();
                int param2 = MethodPackage.Parameter.getParameter(y[1]).getValue();
                string param3 = MethodPackage.Parameter.getParameter(y[2]).getValue();
                string param4 = MethodPackage.Parameter.getParameter(y[3]).getValue();

                string adjectives = this.chainedDraws(
                    param2,
                    Matter.multipleAdjectivesSeperator,
                    getDescribingList(param3.Trim()),
                    Matter.multipleAdjectivesFinalizer
                );

                    

                string alias = this.doDrawFromListString("Aliases", param1);

                string article = "";
                switch (param4.ToUpper().Trim())
                {
                    case "A":
                        article = this.getIndefinateArticle("");
                        break;
                    case "THE":
                        article = "the";
                        break;
                    case "THIS":
                        article = (this.isMany) ? "these" : "this";
                        break;
                    case "THAT":
                        article = (this.isMany) ? "those" : "that";
                        break;
                    case "MY":
                        if(this.Owner != null && this.Owner is Human)
                        {
                            article = (this.Owner as Human).getPossessiveAdjectives();
                        }
                        else article = this.getPossessiveAdjectives();
                        break;
                    case "NONE":
                        article = "";
                        break;
                }

                return article + " " + adjectives + " " + alias;

            });

            //string adjectives = this.chainedDraws(
            //            MethodPackage.Parameter.getParameter(y[0]).getValue(),
            //            Matter.multipleAdjectivesSeperator,
            //            getDescribingList(MethodPackage.Parameter.getParameter(y[1]).getValue()),
            //            Matter.multipleAdjectivesFinalizer
            //            );
            //if (int.TryParse(methodName, out methodNumber))
            //{
            //    if (methodNumber < 3)
            //        return this.chainedAdjectives(methodNumber, ", ");
            //    else
            //        return this.chainedAdjectives(methodNumber, ", ", ", and ");


            #region Legacy List Function
            description = "Gets the next associative value int the list";
            addDelegate("F-S", description, null, (x, y) => { return this.drawFromList(this.who, "", this.WhoPercentage, true); });
            addDelegate("S-S", description, null, (x, y) => { return this.drawFromList(this.states, "", this.StatePercentage, true); });
            addDelegate("V-S", description, null, (x, y) => { return this.drawFromList(this.adverbs, "", this.AdverbsPercentage, true); });
            addDelegate("A-S", description, null, (x, y) => { return this.drawFromList(this.adjectives, "", this.AdjectivesPercentage, true); });
            addDelegate("K-S", description, null, (x, y) => { return this.drawFromList(this.aliases, "", this.AliasPercentage, true); });
            
            description = "Gets the associative value for a random key";
            addDelegate("F-A", description, null, (x, y) => { return evaluateProbability(this.who.getAssociated(), this.WhoPercentage); });
            addDelegate("S-A", description, null, (x, y) => { return evaluateProbability(this.states.getAssociated(), this.StatePercentage); });
            addDelegate("V-A", description, null, (x, y) => { return evaluateProbability(this.adverbs.getAssociated(), this.AdverbsPercentage); });
            addDelegate("A-A", description, null, (x, y) => { return evaluateProbability(this.adjectives.getAssociated(), this.AdjectivesPercentage); });
            addDelegate("K-A", description, null, (x, y) => { return this.aliases.getAssociated(); });
            
            description = "Draws only from items that have associative values, and returns only the associative value.";
            addDelegate("F-F", description, null, (x, y) => { return evaluateProbability(this.who.getAssociated(ListDrawString.DrawingStyle.ForcedSublist), this.WhoPercentage); });
            addDelegate("S-F", description, null, (x, y) => { return evaluateProbability(this.states.getAssociated(ListDrawString.DrawingStyle.ForcedSublist), this.StatePercentage); });
            addDelegate("V-F", description, null, (x, y) => { return evaluateProbability(this.adverbs.getAssociated(ListDrawString.DrawingStyle.ForcedSublist), this.AdverbsPercentage); });
            addDelegate("A-F", description, null, (x, y) => { return evaluateProbability(this.adjectives.getAssociated(ListDrawString.DrawingStyle.ForcedSublist), this.AdjectivesPercentage); });
            addDelegate("K-F", description, null, (x, y) => { return this.aliases.getAssociated(ListDrawString.DrawingStyle.ForcedSublist); });
            
            description = "Returns the last selected item fromt the list.";
            addDelegate("F-L", description, null, (x, y) => { return this.who.LastSelected; });
            addDelegate("S-L", description, null, (x, y) => { return this.states.LastSelected; });
            addDelegate("V-L", description, null, (x, y) => { return this.adverbs.LastSelected; });
            addDelegate("A-L", description, null, (x, y) => { return this.adjectives.LastSelected; });
            addDelegate("K-L", description, null, (x, y) => { return this.aliases.LastSelected; });
            
            description = "Draws only from items that have associative values and returns both key and value like so: {key}{space}{value}";
            addDelegate("F-R", description, null, (x, y) => { return this.who.getAssociated(ListDrawString.DrawingStyle.ForcedBoth); });
            addDelegate("S-R", description, null, (x, y) => { return this.states.getAssociated(ListDrawString.DrawingStyle.ForcedBoth); });
            addDelegate("V-R", description, null, (x, y) => { return this.adverbs.getAssociated(ListDrawString.DrawingStyle.ForcedBoth); });
            addDelegate("K-R", description, null, (x, y) => { return this.aliases.getAssociated(ListDrawString.DrawingStyle.ForcedBoth); });
            addDelegate("A-R", description, null, (x, y) => { return this.adjectives.getAssociated(ListDrawString.DrawingStyle.ForcedBoth); });
            

            #endregion

            parametres = getFreshParameterList();
            addDelegate("EMPTY", "Returns an empty string.", null, (x, y) => { return ""; });
            addDelegate("VIDE", "French for empty. Acts the same as the EMPTY command.", null, (x, y) => { return ""; });
            addDelegate("ISMANY", "Returns either 'true' or 'false' as a string.", null, (x, y) => { return ((this.IsMany) ? "true" : "false"); });
            addDelegate("PA", "Gets the possessive adjective/determinator.", null, (x, y) => { return this.getPossessiveAdjectives(); });
            addDelegate("RP", "Gets the reflextive pronoun.", null, (x, y) => { return this.getReflexivePronouns(); });
            addDelegate("DA", "Demonstrative adjective. The pronoun giving is contingient on the pov and if the object is considered as many objects.", null, (x, y) => { return (this.isMany) ? "these" : "this"; });
            addDelegate("DAFAR", "Demonstrative adjective. Those or that.", null, (x, y) => { return (this.isMany) ? "those" : "that"; });
            addDelegate("DACLOSE", "Demonstrative adjective. This or these", null, (x, y) => { return (this.isMany) ? "these" : "this"; });
            addDelegate("G", "Returns the group ID digit.", null, (x, y) => { return this.GroupId.ToString(); });
            addDelegate("AOP", "Adverb of place", null, (x, y) => { return "that " + this.drawWho(); });
            addDelegate("FF", "Returns 'that' with a draw from the fact list.", null, (x, y) => { return "there"; });
            addDelegate("FSP", "Returns 'that' with a draw from the fact list.", null, (x, y) => { return "who " + this.drawWho(); });
            addDelegate("FOP", "Returns 'that' with a draw from the fact list.", null, (x, y) => { return "that " + this.drawWho(); });
            addDelegate("POV", "Returns an integer that represents the point of view of the character.", null, (x, y) => { return "3"; });

            addDelegate("PP", "", null, (x, y) => { return this.getPossesivePronoun(); });
            addDelegate("PPP", "", null, (x, y) => {
                if (this.Name.EndsWith("s")) return this.Name + "'";
                return this.Name + "'s"; 
            });
            addDelegate("SP", "", null, (x, y) => { return this.getSubjectPronoun(); });
            addDelegate("SPP", "", null, (x, y) => { return this.Name; });
            addDelegate("OP", "", null, (x, y) => { return this.getObjectPronouns(); });
            addDelegate("OPP", "", null, (x, y) => { return this.Name; });
            addDelegate("PAP", "", null, (x, y) => {
                if (this.Name.EndsWith("s")) return this.Name + "'";
                return this.Name + "'s"; 
            });

            //==========================================================================================================
            method = (methodName, parameters) =>
            {
                // Remove the W and pass the code along to the owner object
                if (this.Owner != null && this.Owner is Matter)
                    return (this.Owner as Matter).interpret(methodName.Remove(0, 1), tense);
                return this.interpret(methodName.Remove(0, 1), tense);
            };
            parametres = getFreshParameterList();
            addDelegate("WSP", "Returns the parent's SP code.", parametres, method);
            addDelegate("WOP", "Returns the parent's OP code.", parametres, method);
            addDelegate("WPA", "Returns the parent's PA code.", parametres, method);
            addDelegate("WPP", "Returns the parent's PP code.", parametres, method);
            addDelegate("WRP", "Returns the parent's RP code.", parametres, method);
            addDelegate("WSPP", "Returns the parent's SPP code.", parametres, method);
            addDelegate("WOPP", "Returns the parent's OPP code.", parametres, method);
            addDelegate("WPAP", "Returns the parent's PAP code.", parametres, method);
            addDelegate("WPPP", "Returns the parent's PPP code.", parametres, method);
            addDelegate("WRPP", "Returns the parent's RPP code.", parametres, method);
            //==========================================================================================================
            parametres = getFreshParameterList();
            parametres.Add(new MethodPackage.Parameter("key", "The key needed to match the list with.", typeof(string)));
            description = "Gets the associated item given the key supplied.";
            addDelegate("S", description, parametres, (x, y) => { return this.states.fetch(y[0]);});
            addDelegate("K", description, parametres, (x, y) => { return this.aliases.fetch(y[0]); });
            addDelegate("V", description, parametres, (x, y) => { return this.adverbs.fetch(y[0]); });
            addDelegate("F", description, parametres, (x, y) => { return this.who.fetch(y[0]); });
            addDelegate("A", description, parametres, (x, y) => { return this.adjectives.fetch(y[0]); });
            //==========================================================================================================
            parametres = getFreshParameterList();
            parametres.Add(new MethodPackage.Parameter("Whole Number", "The whole number to convert to text.", typeof(int)));
            addDelegate("NUMBERTOTEXT", "Converts a number in text form.", parametres, (methodName, parameters) =>
            {
                var para = MethodPackage.Parameter.getParameter(parameters[0]);
                return this.getNumberInWordForm((int)para.getValue());
            });
            //==========================================================================================================
            parametres = getFreshParameterList();
            parametres.Add(new MethodPackage.Parameter("Adjective", "The adjective to convert to adverb", typeof(string)));
            addDelegate("ADVERB", "Converts most adjectives to adverbs. Simply adds -ly to word. Useful to use on existing list of adjectives.", parametres, (x, y) => { return Adverb.adjectiveToAdverb(y[0]); });
            //==========================================================================================================
            parametres = getFreshParameterList();
            parametres.Add(new MethodPackage.Parameter("verb", "The verb to convert to an adverb, in infinitive form.", typeof(string)));
            addDelegate("verbToAdverb".ToUpper(), "Converts most verbs to adverbs. Useful with a list of predefined verbs to apply to.", parametres, (x, y) => { return Adverb.adjectiveToAdverb(y[0]); });

            //==========================================================================================================
            addDelegate("ART", "Gets a noun phrase starting with an indefinate article.", getFreshParameterList(), delegate(string methodName, List<string> parameters)
            {
                ListDrawString list = new ListDrawString(this.adjectives.DrawRate);
                list.AddRange(this.states);
                list.AddRange(this.adjectives);
                // has to be interpreted to ensure that whatever the list spits out is not a code to be interpreted. 
                string adjectivesAsString = this.interpret(this.chainedDraws(1, multipleAdjectivesSeperator, list, multipleAdjectivesFinalizer), this.tense);
                string selectedMoniker = this.interpret("k", this.tense);
                return this.getIndefinateArticle(selectedMoniker, adjectivesAsString);
            });
            //==========================================================================================================

            #region $@+%
            //==========================================================================================================
            addDelegate("@", "Objective noun phrase if the character is in the third person. Else returns a standard objective pronoun. (I, you)", null, (x, y) =>
            {
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
            parametres = getFreshParameterList(); parametres.Add(new MethodPackage.Parameter("Adjective Count", "The amount of adjectives to be drawn.", typeof(int)));
            addDelegate("@", "Objective noun phrase if the character is in the third person. Else returns a standard objective pronoun. (I, you)", parametres, (x, y) =>
            {
                if (willDraw(this.AliasPercentage))
                {
                    string da = "the";
                    string adjectives = this.chainedDraws(MethodPackage.Parameter.getParameter(y[0]).getValue(), Matter.multipleAdjectivesSeperator, getDescribingList(ListCode.Adjective), Matter.multipleAdjectivesFinalizer);
                    string moniker =  this.interpret("k-a", tense);
                    if (!String.IsNullOrWhiteSpace(adjectives)) return da + " " + adjectives + " " + moniker; else return this.interpret("op", tense);
                }
                else
                {
                    return "the " + this.interpret("k-a", tense);
                }
            });
            //==========================================================================================================
            parametres = getFreshParameterList();
            parametres.Add(new MethodPackage.Parameter("Adjective Count", "The amount of adjectives to be drawn.", typeof(int)));
            parametres.Add(new MethodPackage.Parameter("Mode", "Lists to use. M,A,V,S,AS,SA,VA,AV,VS,SV,SAV", typeof(string)));
            addDelegate("@", "Objective noun phrase if the character is in the third person. Else returns a standard objective pronoun. (I, you)", parametres, (x, y) =>
            {
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
                    if (!String.IsNullOrWhiteSpace(adjectives) && !String.IsNullOrWhiteSpace(moniker)) return da + " " + adjectives + " " + moniker; else return this.interpret("op", tense);
                }
                else
                {
                    return this.interpret("@", tense);
                }
            });
            //==========================================================================================================
            addDelegate("$", "Subjective noun phrase if the character is in the third person. Else returns a standard subjective pronoun. (I, you)", null, (x, y) =>
            {
                if (willDraw(this.AliasPercentage))
                {
                    string da = "the"; //this.interpret("da", tense);
                    string yar = this.interpret("yar", tense);
                    if (!String.IsNullOrWhiteSpace(yar)) return da + " " + yar; else return this.interpret("sp", tense);
                    //if (!String.IsNullOrWhiteSpace(yar))
                    //{
                    //    if (yar.EndsWith("s")) return "the " + yar + "'";
                    //    return "the " + yar + "'s";
                    //}
                    //else return this.interpret("sp", tense);
                }
                else
                {
                    return "the " + this.interpret("k-a", tense);
                }
            });
            //==========================================================================================================
            parametres = getFreshParameterList(); parametres.Add(new MethodPackage.Parameter("Adjective Count", "The amount of adjectives to be drawn.", typeof(int)));
            addDelegate("$", "Subjective noun phrase if the character is in the third person. Else returns a standard subjective pronoun. (I, you)", parametres, (x, y) =>
            {
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
            addDelegate("+", "Possessive noun phrase if the character is in the third person. Returns standard possesive adjective otherwise. (my, your)", null, (x, y) =>
            {
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
            parametres = getFreshParameterList(); parametres.Add(new MethodPackage.Parameter("Adjective Count", "The amount of adjectives to be drawn.", typeof(int)));
            addDelegate("+", "Possessive noun phrase if the character is in the third person. Returns standard possesive adjective otherwise. (my, your)", parametres, (x, y) =>
            {
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
                    //return "the " + this.interpret("k-a", tense) + "'s";
                }
            });
            //==========================================================================================================
            addDelegate("%", "Possessive noun phrase if the character is in the third person. Returns standard possesive pronoun otherwise. (mine, yours)", null, (x, y) =>
            {
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
            //==========================================================================================================
            parametres = getFreshParameterList(); parametres.Add(new MethodPackage.Parameter("Adjective Count", "The amount of adjectives to be drawn.", typeof(int)));
            addDelegate("%", "Possessive noun phrase if the character is in the third person. Returns standard possesive pronoun otherwise. (mine, yours)", parametres, (x, y) =>
            {
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
                    //return "the " + this.interpret("k-a", tense) + "'s";
                }

            });
            //==========================================================================================================
            addDelegate("$$$", "returns a 'the' noun phrase, but failsafes using SPP and ensures that adjectives do not contain the noun within them. E.g. Godlike god", null, (x, y) => { return tripleDollarSign(); });
            //==========================================================================================================
            parametres = getFreshParameterList(); parametres.Add(new MethodPackage.Parameter("Adjective Count", "The amount of adjectives to be drawn.", typeof(int)));
            addDelegate("$$$", "Something", parametres, (x, y) => { return tripleDollarSign(MethodPackage.Parameter.getParameter(y[0]).getValue()); });
            //==========================================================================================================
            addDelegate("@@@", "Something", null, (x, y) => { return tripleAtSign(); });
            //==========================================================================================================
            parametres = getFreshParameterList(); parametres.Add(new MethodPackage.Parameter("Adjective Count", "The amount of adjectives to be drawn.", typeof(int)));
            addDelegate("@@@", "Something", parametres, (x, y) => { return tripleAtSign(MethodPackage.Parameter.getParameter(y[0]).getValue()); });
            //==========================================================================================================
            addDelegate("$$", "Something", null, (x, y) => { return referenceMeByOther("ref" + this.Id, "$", new string[] { "~the", "as", "k" }, tense); });
            addDelegate("@@", "Something", null, (x, y) => { return referenceMeByOther("ref" + this.Id, "@", new string[] { "da", "as", "k" }, tense); });
            addDelegate("++", "Something", null, (x, y) => { return referenceMeByOther("ref" + this.Id, "+", new string[] { "wpa", "as", "k" }, tense); });
            addDelegate("%%", "Something", null, (x, y) => { return referenceMeByOther("ref" + this.Id, "%", new string[] { "~the", "as", "k", "~of", "wpp" }, tense); });

            #endregion

            
            #region $@+% With Mode
            
            //==========================================================================================================
            parametres = getFreshParameterList();
            parametres.Add(new MethodPackage.Parameter("Adjective Count", "The amount of adjectives to be drawn.", typeof(int)));
            parametres.Add(new MethodPackage.Parameter("Mode", "Lists to use. M,A,V,S,AS,SA,VA,AV,VS,SV,SAV", typeof(string)));
            addDelegate("$", "Subjective noun phrase if the character is in the third person. Else returns a standard subjective pronoun. (I, you)", parametres, (x, y) =>
            {
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
                    if (!String.IsNullOrWhiteSpace(adjectives) && !String.IsNullOrWhiteSpace(moniker)) return da + " " + adjectives + " " + moniker; else return this.interpret("sp", tense);
                }
                else
                {
                    return this.interpret("$", tense);
                }
            });
            //==========================================================================================================
            parametres = getFreshParameterList();
            parametres.Add(new MethodPackage.Parameter("Adjective Count", "The amount of adjectives to be drawn.", typeof(int)));
            parametres.Add(new MethodPackage.Parameter("Mode", "Lists to use. M,A,V,S,AS,SA,VA,AV,VS,SV,SAV", typeof(string)));
            addDelegate("+", "Possessive noun phrase if the character is in the third person. Returns standard possesive adjective otherwise. (my, your)", parametres, (x, y) =>
            {
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
                    if (!String.IsNullOrWhiteSpace(adjectives) && !String.IsNullOrWhiteSpace(moniker)) return da + " " + adjectives + " " + moniker; else return this.interpret("pa", tense);
                }
                else
                {
                    string moniker = this.interpret("k-a", tense); moniker += (moniker.EndsWith("s")) ? "'" : "'s";
                    return "the " + moniker;
                    //return "the " + this.interpret("k-a", tense) + "'s";
                }
            });
            //==========================================================================================================
            parametres = getFreshParameterList();
            parametres.Add(new MethodPackage.Parameter("Adjective Count", "The amount of adjectives to be drawn.", typeof(int)));
            parametres.Add(new MethodPackage.Parameter("Mode", "Lists to use. M,A,V,S,AS,SA,VA,AV,VS,SV,SAV", typeof(string)));
            addDelegate("%", "Possessive noun phrase if the character is in the third person. Returns standard possesive pronoun otherwise. (mine, yours)", parametres, (x, y) =>
            {
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
                    if (!String.IsNullOrWhiteSpace(adjectives) && !String.IsNullOrWhiteSpace(moniker)) return da + " " + adjectives + " " + moniker; else return this.interpret("pp", tense);
                }
                else
                {
                    string moniker = this.interpret("k-a", tense); moniker += (moniker.EndsWith("s")) ? "'" : "'s";
                    return "the " + moniker;
                    //return "the " + this.interpret("k-a", tense) + "'s";
                }

            });
            #endregion

            #region Verb Methods
            parametres = getFreshParameterList();
            parametres.Add(new MethodPackage.Parameter("Infinitive", "The verb in infinitive form.", typeof(string)));

            addDelegate("V", "Verbs", parametres, (x, y) =>
            {
                var relativeTense = Verb.getTenseFromInt(this.tense);
                var verb = new Verb(y[0], this.isMany, 3, Classes.Verb.Gender.it, relativeTense);
                return verb.getVerb();
            });

            parametres = getFreshParameterList();
            parametres.Add(new MethodPackage.Parameter("Infinitive", "The verb in infinitive form.", typeof(string)));
            parametres.Add(new MethodPackage.Parameter("Relative tense", "Values accepted are -1, 0, 1, 2", typeof(int)));
            addDelegate("V", "Verbs", parametres, (x, y) =>
            {
                int secondParameter = MethodPackage.Parameter.getParameter(y[1]).getValue();
                if (secondParameter < -1 || secondParameter > 2) secondParameter = 0;
                var relativeTense = Verb.getTenseFromInt(this.tense + secondParameter);
                //var relativeTense = Verb.getTenseFromInt(this.tense + MethodPackage.Parameter.getParameter(y[1]).getValue());
                var verb = new Verb(y[0], this.isMany, 3, Classes.Verb.Gender.it, relativeTense);
                return verb.getVerb();
            });

            parametres = getFreshParameterList();
            parametres.Add(new MethodPackage.Parameter("Infinitive", "The verb in infinitive form.", typeof(string)));
            parametres.Add(new MethodPackage.Parameter("Relative tense", "Values accepted are -1, 0, 1, 2", typeof(int)));
            parametres.Add(new MethodPackage.Parameter("Adverb", "And adverb to slap into the verb phrase.", typeof(string)));
            addDelegate("V", "Verbs", parametres, (x, y) =>
            {
                int secondParameter = MethodPackage.Parameter.getParameter(y[1]).getValue();
                if (secondParameter < -1 || secondParameter > 2) secondParameter = 0;
                var relativeTense = Verb.getTenseFromInt(this.tense + secondParameter);
                //var relativeTense = Verb.getTenseFromInt(this.tense + MethodPackage.Parameter.getParameter(y[1]).getValue());
                var verb = new Verb(y[0], this.isMany, 3, Classes.Verb.Gender.it, relativeTense, y[2]);
                return verb.getVerb();
            });
            parametres = getFreshParameterList();
            parametres.Add(new MethodPackage.Parameter("Infinitive", "The verb in infinitive form.", typeof(string)));
            parametres.Add(new MethodPackage.Parameter("Adverb", "And adverb to slap into the verb phrase.", typeof(string)));
            addDelegate("V", "Verbs", parametres, (x, y) =>
            {
                int parameteredRelativeTense = Verb.getIntFromString(y[1]);
                Verb verb = null;
                if (parameteredRelativeTense != -1)
                {
                    verb = new Verb(y[0], this.isMany, 3, Classes.Verb.Gender.it, Verb.getTenseFromInt(parameteredRelativeTense));
                }
                else
                {
                    verb = new Verb(y[0], this.isMany, 3, Classes.Verb.Gender.it, Verb.getTenseFromInt(this.tense), y[1]);
                }
                
                return verb.getVerb();
            });

            parametres = getFreshParameterList();
            parametres.Add(new MethodPackage.Parameter("Infinitive", "The verb in infinitive form.", typeof(string)));
            parametres.Add(new MethodPackage.Parameter("Constant Tense", "The tense constant name in string form. Case sensitive.", typeof(string)));
            parametres.Add(new MethodPackage.Parameter("Adverb", "And adverb to slap into the verb phrase.", typeof(string)));
            addDelegate("V", "Verbs", parametres, (x, y) =>
            {
                int parameteredRelativeTense = Verb.getIntFromString(y[1]);
                Verb verb = null;
                if (parameteredRelativeTense != -1)
                {
                    parameteredRelativeTense = Verb.getIntFromString("Continuous");
                    
                }
                verb = new Verb(y[0], this.isMany, 3, Classes.Verb.Gender.it, Verb.getTenseFromInt(parameteredRelativeTense), y[2]);
                return verb.getVerb();
            });

            #endregion




        }

        protected string doDrawFromListString(string list, string operation)
        {
            ListDrawString listToDrawFrom = null;
            int percentage = 100;

            switch (list.ToUpper())
            {
                case "FACT":
                    listToDrawFrom = this.who;
                    percentage = this.WhoPercentage;
                    break;
                case "FACTS":
                    listToDrawFrom = this.who;
                    percentage = this.WhoPercentage;
                    break;
                case "F":
                    listToDrawFrom = this.who;
                    percentage = this.WhoPercentage;
                    break;
                case "A":
                    listToDrawFrom = this.adjectives;
                    percentage = this.AdjectivesPercentage;
                    break;
                case "ADJ":
                    listToDrawFrom = this.adjectives;
                    percentage = this.AdjectivesPercentage;
                    break;
                case "ADJECTIVES":
                    listToDrawFrom = this.adjectives;
                    percentage = this.AdjectivesPercentage;
                    break;
                case "ADJECTIVE":
                    listToDrawFrom = this.adjectives;
                    percentage = this.AdjectivesPercentage;
                    break;
                case "V":
                    listToDrawFrom = this.adverbs;
                    percentage = this.AdverbsPercentage;
                    break;
                case "ADV":
                    listToDrawFrom = this.adverbs;
                    percentage = this.AdverbsPercentage;
                    break;
                case "ADVERB":
                    listToDrawFrom = this.adverbs;
                    percentage = this.AdverbsPercentage;
                    break;
                case "ADVERBS":
                    listToDrawFrom = this.adverbs;
                    percentage = this.AdverbsPercentage;
                    break;
                case "STATE":
                    listToDrawFrom = this.states;
                    percentage = this.StatePercentage;
                    break;
                case "STATES":
                    listToDrawFrom = this.states;
                    percentage = this.StatePercentage;
                    break;
                case "S":
                    listToDrawFrom = this.states;
                    percentage = this.StatePercentage;
                    break;
                case "ALIAS":
                    listToDrawFrom = this.aliases;
                    percentage = this.AliasPercentage;
                    break;
                case "ALIASES":
                    listToDrawFrom = this.aliases;
                    percentage = this.AliasPercentage;
                    break;
                case "K":
                    listToDrawFrom = this.aliases;
                    percentage = this.AliasPercentage;
                    break;
            }


            switch (operation.ToUpper())
            {
                case "S":
                    return this.drawFromList(listToDrawFrom, "", percentage, true);
                case "SEQUENCIALLY":
                    return this.drawFromList(listToDrawFrom, "", percentage, true);
                case "LINEAR":
                    return this.drawFromList(listToDrawFrom, "", percentage, true);
                case "LINEARLY":
                    return this.drawFromList(listToDrawFrom, "", percentage, true);
                case "L":
                    return listToDrawFrom.LastSelected;
                case "LAST":
                    return listToDrawFrom.LastSelected;
                case "PREVIOUS":
                    return listToDrawFrom.LastSelected;
                case "V":
                    return evaluateProbability(listToDrawFrom.getAssociated(), percentage);
                case "VALUE":
                    return evaluateProbability(listToDrawFrom.getAssociated(), percentage);
                case "F":
                    return evaluateProbability(listToDrawFrom.getAssociated(ListDrawString.DrawingStyle.ForcedSublist), percentage);
                case "FORCED":
                    return evaluateProbability(listToDrawFrom.getAssociated(ListDrawString.DrawingStyle.ForcedSublist), percentage);
                case "B":
                    return listToDrawFrom.getAssociated(ListDrawString.DrawingStyle.ForcedBoth);
                case "BOTH":
                    return listToDrawFrom.getAssociated(ListDrawString.DrawingStyle.ForcedBoth);
                case "K":
                    return listToDrawFrom.draw();
                case "KEY":
                    return listToDrawFrom.draw();

            }

            try
            {
                return evaluateProbability(listToDrawFrom.getAssociated(), percentage);
            }
            catch (Exception)
            {

                return "";
            }
        }

        #endregion

        protected int tense = 2;
        protected virtual string tripleDollarSign(int adjectiveCount = 1)
        {
            // Default to always drawing at least one adjective
            if (adjectiveCount < 0) adjectiveCount = 1;
            string adjectives = this.chainedDraws(adjectiveCount, Matter.multipleAdjectivesSeperator, this.adjectives, Matter.multipleAdjectivesFinalizer);

            // Lazily send the $ code instead of just pumping out the strings
            if (String.IsNullOrWhiteSpace(adjectives)) return this.interpret("$", tense);
            // Set the failsafe code
            string interpretedFailsafe = this.interpret("SPP", tense);
            // return the noun phrase
            return "the " + this.drawNoConflict(adjectives, this.aliases, tense, interpretedFailsafe); 
        }

        protected virtual string tripleAtSign(int adjectiveCount = 1)
        {
            if (adjectiveCount < 0) adjectiveCount = 1;
            string adjectives = this.chainedDraws(adjectiveCount, Matter.multipleAdjectivesSeperator, this.adjectives, Matter.multipleAdjectivesFinalizer);
            if (String.IsNullOrWhiteSpace(adjectives)) return this.interpret("@", tense);
            string interpretedFailsafe = this.interpret("OPP", tense);
            return "the " + this.drawNoConflict(adjectives, this.aliases, tense, interpretedFailsafe);
        }

        private string getDemonstrativeAdjective()
        {
            if (this.getOwner() != null)
            {
                var owner = this.getOwner();
                if (owner is Human)
                {
                    var humanOwner = owner as Human;
                    if (humanOwner.Perspective == 3 && humanOwner.isPov)
                    {
                        if (this.isMany)
                            return "these";
                        else
                            return "this";
                    }
                    else
                    {
                        if (this.isMany)
                            return "those";
                        else
                            return "that";
                    }
                }
            }
            if (this.isMany)
                return "these";
            else
                return "this";
        }

        public virtual string interpret(string code, int tense)
        {
            if (String.IsNullOrWhiteSpace(code)) return "";
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

            // If the matter contains a number as a method name, then apply the default random draw and draw the amount the number indicates
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

            // ======================================================================Legacy=======================================================================

            //if (code.StartsWith("{") && code.EndsWith("}"))
            //{
            //    code = code.Remove(code.Length - 1, 1).Remove(0, 1);
            //}

            if (String.IsNullOrWhiteSpace(code)) return String.Empty;


            if (code.ToUpper().StartsWith("*"))
            {
                code = code.Remove(0, 1);
                string[] split = code.Split(',');
                string finalstring = "";
                foreach (string str in split)
                {
                    finalstring += this.interpret(str, tense) + " ";
                }
                return finalstring.Trim();
            }

            // Advance list calls having associative arrays
            if (regexCallListWithKey.IsMatch(code.ToUpper()))
            {
                ListDrawString list = null;
                string theCode = code.ToUpper();
                //if (code.StartsWith("SC")) list = this.scripts;
                if (theCode.StartsWith("A")) list = this.adjectives;
                if (theCode.StartsWith("S")) list = this.states;
                if (theCode.StartsWith("K")) list = this.aliases;
                if (theCode.StartsWith("F")) list = this.who;
                if (theCode.StartsWith("V")) list = this.adverbs;

                string listCode = theCode.Remove(0, 1);
                listCode = listCode.Remove(listCode.Length - 1, 1);

                return list.fetch(listCode, new string[]{"."});

            }

            // Number to text
            if (regexIsNumberToText.IsMatch(code))
            {
                string numberAsString = code.Remove(0, 13);
                numberAsString = numberAsString.Remove(numberAsString.Length - 1, 1);

                int number = -1;
                int.TryParse(numberAsString, out number);

                if (number == -1) return numberAsString;

                return this.getNumberInWordForm(number);
            }

            if (regexIsAdverb.IsMatch(code))
            {
                string adverb = code.Remove(0, 7);
                adverb = adverb.Remove(adverb.Length - 1, 1);

                return Adverb.adjectiveToAdverb(adverb);
            }

            if (regexIsVerbToAdverb.IsMatch(code))
            {
                string adverb = code.Remove(0, 13);
                adverb = adverb.Remove(adverb.Length - 1, 1);

                return Adverb.verbToAdverb(adverb);
            }

            string resultSetProperty = setProperty(code);
            if (String.IsNullOrWhiteSpace(resultSetProperty)) return "";

            string resultGetProperty = getProperty(code);
            if (!String.IsNullOrWhiteSpace(resultGetProperty)) return resultGetProperty;


            Verb.Gender gender = Verb.Gender.it;
            string verb = Verb.getVerb(code, false, 3, true, gender, tense);
            if (!String.IsNullOrWhiteSpace(verb)) return verb;


            int adjectiveCount = 0;

            if (code.ToUpper().StartsWith("~"))
                return code.Remove(0, 1);

            if (code.ToUpper().StartsWith("*"))
            {
                code = code.Remove(0, 1);
                string[] split = code.Split(',');
                string finalstring = "";
                foreach (string str in split)
                {
                    finalstring += this.interpret(str, tense) + " ";
                }
                return finalstring.Trim();
            }

            string resultArticle = getArticle(code, tense);
            if (!String.IsNullOrWhiteSpace(resultArticle)) return resultArticle;


            // if the code is a number
            if (int.TryParse(code, out adjectiveCount))
            {
                if (adjectiveCount < 3)
                    return this.chainedAdjectives(adjectiveCount, ", ");
                else
                    return this.chainedAdjectives(adjectiveCount, ", ", ", and ");
            }

            //Regex quickMonikers = new Regex(@"^(\@|\+|\%|\$|\$\$|\@\@|\+\+|\%\%){1}.*$");
            if (quickMonikers.IsMatch(code))
            {


                if (code.StartsWith("@@@") || code.StartsWith("$$$"))
                {
                    if (willDraw(this.AliasPercentage))
                    {

                        code = methodName;
                        if (code.Length > 3) code = methodName.Remove(0, 3);
                        return multiAdjectiveNounPhrase(code, methodName[0].ToString(), "the", tense, this.aliases); 
                    }
                    else return this.interpret(methodName[0].ToString(), tense);

                }


                // !@#$%^&*()_+
                if (code.StartsWith("$$")) return this.refOther(code, "$", new string[] { "~the", "as", "k" }, tense);
                if (code.StartsWith("@@")) return this.refOther(code, "@", new string[] { "da", "as", "k" }, tense);
                if (code.StartsWith("++")) return this.refOther(code, "+", new string[] { "wpa", "as", "k" }, tense);
                if (code.StartsWith("%%")) return this.refOther(code, "%", new string[] { "~the", "as", "k", "~of", "wpp" }, tense);

                if (code.StartsWith("@"))
                {
                    if (code.Length > 1)
                    {
                        code = methodName.Remove(0, 1);
                        return multiAdjectiveNounPhrase(code, "@", "the", tense, this.aliases);
                        //return this.interpret("*" + code); // Star (*) functionality is located in Matter. It replaces commas with spaces: A runtime command... 
                    }
                    else
                    {
                        if (willDraw(this.AliasPercentage))
                        {
                            string output = this.interpret("*da,yar", tense);
                            if (output.Length > this.interpret("da", tense).Length) return output; else return this.interpret("op", tense);
                        }
                        else
                        {
                            if (this is Human) return this.interpret("op", tense);
                            return this.interpret("*da,k", tense);
                        }
                    }
                }

                if (code.StartsWith("$"))
                {
                    if (code.Length > 1)
                    {
                        code = methodName.Remove(0, 1);
                        return multiAdjectiveNounPhrase(code, "$", "the", tense, this.aliases);
                        //return this.interpret("*" + code);
                    }
                    else
                    {
                        if (willDraw(this.AliasPercentage))
                        {
                            string output = "the " + this.interpret("yar", tense);
                            if (output.Length > 4) return output; else return this.interpret("sp", tense);
                        }
                        else
                        {
                            if (this is Human) return this.interpret("sp", tense);
                            return "the " + this.interpret("k", tense);
                        }
                    }
                }

                if (code.StartsWith("+"))
                {
                    if (code.Length > 1)
                    {
                        code = methodName.Remove(0, 1);
                        return this.interpret("*" + code, tense);
                    }
                    else
                    {
                        if (willDraw(this.AliasPercentage))
                        {
                            string output = "the " + this.interpret("yar", tense) + "'s";
                            if (output.Length > 6) return output; else return this.interpret("pa", tense);
                        }
                        else
                        {
                            if (this is Human) return this.interpret("pa", tense) + this.interpret("k-a", tense) + "'s";
                            return "the " + this.interpret("k-a", tense) + "'s";
                        }
                    }
                }

                if (code.StartsWith("%"))
                {
                    if (code.Length > 1)
                    {
                        code = methodName.Remove(0, 1);
                        return this.interpret("*" + code, tense);
                    }
                    else
                    {
                        if (willDraw(this.AliasPercentage))
                        {
                            string output = "the " + this.interpret("yar", tense) + "'s";
                            if (output.Length > 6) return output; else return this.interpret("pp", tense);
                        }
                        else
                        {
                            if (this is Human) return this.interpret("pp", tense);
                            return "the " + this.interpret("k", tense) + "'s";
                        }
                    }
                }
            }


            // Chained words. Use single digit to 
            // ex: A1 
            if ((new Regex(@"^(S|A|K|F|V)\d$", RegexOptions.IgnoreCase)).IsMatch(code.ToUpper()))
            {
                string codeUpper = code.ToUpper();
                ListDrawString list = null;
                switch (codeUpper.Substring(0, 1))
                {
                    case "S":
                        list = this.states;
                        break;
                    case "A":
                        list = this.adjectives;
                        break;
                    case "V":
                        list = this.adverbs;
                        break;
                    case "F":
                        list = this.who;
                        break;
                    case "K":
                        list = this.aliases;
                        break;
                }

                if (list == null) return "";
                int amount = 0;
                codeUpper = codeUpper.Remove(0, 1);
                if (int.TryParse(codeUpper, out amount))
                {
                    if (amount < 2) return list.draw();
                    return this.chainedDraws(amount, ", ", list, ", and ");
                }
                else return list.draw();

            }

            switch (code.ToUpper())
            {
                case "N":
                    return this.Name;
                case "ID":
                    return this.getFullId();
                case "C":
                    return this.Colour;
                case "Z":
                    return this.Size;
                case "A":
                    return this.adjectives.draw();
                //return this.drawFromList(this.adjectives, "", this.AdjectivesPercentage);// this.chainedAdjectives(1, ""); // + " " + getAliasName();
                case "A-N":
                    return this.drawFromList(this.adjectives, "", this.AdjectivesPercentage, true);// this.chainedAdjectives(1, ""); // + " " + getAliasName();
                case "A-A": // gets an associated value if one exists. Otherwise, the key is selected
                    return evaluateProbability(this.adjectives.getAssociated(ListDrawString.DrawingStyle.Sublist), this.AdjectivesPercentage);
                case "A-F": // forces an associated value. If list contains none, an empty string is returned.
                    return evaluateProbability(this.adjectives.getAssociated(ListDrawString.DrawingStyle.ForcedSublist), this.AdjectivesPercentage);
                case "A-L": // gets the last adjectinve selected.
                    return this.adjectives.LastSelected;
                case "A-R":
                    return this.adjectives.getAssociated(ListDrawString.DrawingStyle.ForcedBoth);
                //return getAdverbAdjective(this.adjectives);
                case "A2":
                    return this.chainedAdjectives(2, justANDseparator); // + " " + getAliasName();
                case "A3":
                    return this.chainedAdjectives(3, justANDseparator); // + " " + getAliasName();
                case "A4":
                    return this.chainedAdjectives(4, justANDseparator);
                case "S2":
                    return this.chainedDraws(2, justANDseparator, this.states, Matter.multipleAdjectivesFinalizer);
                case "SA":
                    string state = this.drawFromList(this.states, "", this.StatePercentage);
                    string adj = this.chainedAdjectives(1, "");
                    if (String.IsNullOrWhiteSpace(state))
                    {
                        if (String.IsNullOrWhiteSpace(adj)) return "";
                        return adj;
                    }
                    if (String.IsNullOrWhiteSpace(adj)) return state;
                    return state + Matter.justANDseparator + this.chainedAdjectives(1, "");
                case "AS":
                    string state1 = this.drawFromList(this.states, "", this.StatePercentage);
                    string adjective1 = this.chainedAdjectives(1, "");
                    if (String.IsNullOrWhiteSpace(state1)) return adjective1;
                    if (String.IsNullOrWhiteSpace(adjective1)) return state1;
                    int ran = this.r.Next(0, 10);
                    return ((ran < 7) ? adjective1 : state1);
                case "MOI":
                    return drawMoi("XX", "$", tense);
                case "TOI":
                    return drawMoi("XX", "@", tense);
                case "MON":
                    return drawMon("XX", "+", tense);
                case "K":
                    //string defaultString = (this is Human) ? "TYPE" : "N";
                    string monikerFound = this.aliases.draw();
                    if (String.IsNullOrWhiteSpace(monikerFound)) return this.aliasFailSafe();
                    return monikerFound;
                //return drawFromList(this.aliases, this.interpret(defaultString), this.AliasPercentage);
                case "K-N":
                    return drawFromList(this.aliases, this.interpret(this.aliasFailSafe(), tense), this.AliasPercentage, true);
                case "K-A":
                    return drawFromList(this.aliases, this.interpret(this.aliasFailSafe(), tense), this.AliasPercentage);
                case "K-F":
                    return evaluateProbability(this.aliases.getAssociated(ListDrawString.DrawingStyle.ForcedSublist), this.AliasPercentage);
                case "K-L":
                    return this.aliases.LastSelected;
                case "K-R":
                    return this.aliases.getAssociated(ListDrawString.DrawingStyle.ForcedBoth);
                //return getAdverbAdjective(this.aliases);
                case "Y":
                    string eitherOr = (this.r.Next(0, 100) > 50) ? "A-A" : "S-A";
                    return this.drawNoConflict(this.interpret(eitherOr, tense), this.aliases, tense, this.aliasFailSafe());
                case "YAR":
                    return this.drawNoConflict(this.interpret("A-A", tense), this.aliases, tense, this.aliasFailSafe());
                case "YSR":
                    return this.drawNoConflict(this.interpret("S-A", tense), this.aliases, tense, this.aliasFailSafe());
                case "W":
                    return this.Weight.ToString();
                case "W-W":
                    return this.getNumberInWordForm(this.Weight);
                case "W-WH":
                    return this.getNumberInWordForm(this.Weight).Replace(' ', '-');
                case "OTHER":
                    return this.other;
                case "S":
                    string state2 = this.states.draw();  // this.drawFromList(this.states, "", this.StatePercentage);
                    if (String.IsNullOrWhiteSpace(state2)) return this.chainedAdjectives(1, "");
                    return state2;
                case "S-N":
                    string state3 = this.drawFromList(this.states, "", this.StatePercentage, true);
                    if (String.IsNullOrWhiteSpace(state3)) return this.chainedAdjectives(1, "");
                    return state3;
                case "S-A":
                    return evaluateProbability(this.states.getAssociated(), this.StatePercentage);
                case "S-F":
                    return evaluateProbability(this.states.getAssociated(ListDrawString.DrawingStyle.ForcedSublist), this.StatePercentage);
                case "S-L":
                    return this.states.LastSelected;
                case "S-R":
                    return this.states.getAssociated(ListDrawString.DrawingStyle.ForcedBoth);
                //return getAdverbAdjective(this.states);
                case "V":
                    return this.drawFromList(this.adverbs, "", this.AdverbsPercentage);
                case "V-N":
                    return this.drawFromList(this.adverbs, "", this.AdverbsPercentage, true);
                case "V-A":
                    return evaluateProbability(this.adverbs.getAssociated(), this.AdverbsPercentage);
                case "V-F":
                    return evaluateProbability(this.adverbs.getAssociated(ListDrawString.DrawingStyle.ForcedSublist), this.AdverbsPercentage);
                case "V-L":
                    return this.adverbs.LastSelected;
                case "V-R":
                    return this.adverbs.getAssociated(ListDrawString.DrawingStyle.ForcedBoth);
                //return getAdverbAdjective(this.adverbs);
                case "F": // stands for fact subject perspective
                    return this.who.draw();
                //return this.drawFromList(this.who, "", this.WhoPercentage);
                case "F-N": // stands for fact subject perspective
                    return this.drawFromList(this.who, "", this.WhoPercentage, true);
                case "F-C": // stands for fact subject perspective
                    return ", " + this.drawFromList(this.who, "", this.WhoPercentage) + ", ";
                case "F-A":
                    return evaluateProbability(this.who.getAssociated(), this.WhoPercentage);
                case "F-F":
                    return evaluateProbability(this.who.getAssociated(ListDrawString.DrawingStyle.ForcedSublist), this.WhoPercentage);
                case "F-L":
                    return this.who.LastSelected;
                case "F-R":
                    return this.who.getAssociated(ListDrawString.DrawingStyle.ForcedBoth);
                //return getAdverbAdjective(this.who);
                case "EMPTY":
                    return "";
                case "VIDE":
                    return "";
                case "ISMANY":
                    return ((this.IsMany) ? "true" : "false");
                case "PA":
                    return this.getPossessiveAdjectives();
                case "RP":
                    return this.getReflexivePronouns();
                case "DA": //demonstrative adjective
                    if (this.getOwner() != null)
                    {
                        var owner = this.getOwner();
                        if (owner is Human)
                        {
                            var humanOwner = owner as Human;
                            if (humanOwner.Perspective == 3 && humanOwner.isPov)
                            {
                                if (this.isMany)
                                    return "these";
                                else
                                    return "this";
                            }
                            else
                            {
                                if (this.isMany)
                                    return "those";
                                else
                                    return "that";
                            }
                        }
                    }
                    if (this.isMany)
                        return "these";
                    else
                        return "this";

                case "DAFAR":
                    if (this.isMany)
                        return "those";
                    else
                        return "that";
                case "DACLOSE":
                    if (this.isMany)
                        return "these";
                    else
                        return "this";
                case "G":
                    return this.GroupId.ToString();
                case "AOP":  // adverb of place
                    return "there";
                case "FF":
                    return "that " + this.drawWho();

                case "FSP": // stands for fact subject perspective
                    return "that " + this.drawWho();

                case "FOP": // stands for fact Object perspective
                    return "that " + this.drawWho();
                case "POV":
                    return "3";
                case "WSP":
                    if (this.Owner != null && this.Owner is Matter)
                        return (this.Owner as Matter).interpret(code.Remove(0, 1), tense);
                    return this.interpret(code.Remove(0, 1), tense);
                case "WOP":
                    if (this.Owner != null && this.Owner is Matter)
                        return (this.Owner as Matter).interpret(code.Remove(0, 1), tense);
                    return this.interpret(code.Remove(0, 1), tense);
                case "WPA":
                    if (this.Owner != null && this.Owner is Matter)
                        return (this.Owner as Matter).interpret(code.Remove(0, 1), tense);
                    return this.interpret(code.Remove(0, 1), tense);

                case "WPP":
                    if (this.Owner != null && this.Owner is Matter)
                        return (this.Owner as Matter).interpret(code.Remove(0, 1), tense);
                    return this.interpret(code.Remove(0, 1), tense);
                case "WRP":
                    if (this.Owner != null && this.Owner is Matter)
                        return (this.Owner as Matter).interpret(code.Remove(0, 1), tense);
                    return this.interpret(code.Remove(0, 1), tense);

                case "WSPP":
                    if (this.Owner != null && this.Owner is Matter)
                        return (this.Owner as Matter).interpret(code.Remove(0, 1), tense);
                    return this.interpret(code.Remove(0, 1), tense);
                case "WOPP":
                    if (this.Owner != null && this.Owner is Matter)
                        return (this.Owner as Matter).interpret(code.Remove(0, 1), tense);
                    return this.interpret(code.Remove(0, 1), tense);
                case "WPAP":
                    if (this.Owner != null && this.Owner is Matter)
                        return (this.Owner as Matter).interpret(code.Remove(0, 1), tense);
                    return this.interpret(code.Remove(0, 1), tense);
                case "WPPP":
                    if (this.Owner != null && this.Owner is Matter)
                        return (this.Owner as Matter).interpret(code.Remove(0, 1), tense);
                    return this.interpret(code.Remove(0, 1), tense);
                case "WRPP":
                    if (this.Owner != null && this.Owner is Matter)
                        return (this.Owner as Matter).interpret(code.Remove(0, 1), tense);
                    return this.interpret(code.Remove(0, 1), tense);



                default:
                    return code;
            }


        }

        private string aliasFailSafe()
        {
            if (this is Human)
            {
                return this.interpret("TYPE", 2);
            }
            else
            {
                return this.interpret("N", 2);
            }
        }

        public virtual Dictionary<string, string> help()
        {
            Dictionary<string, string> d = new Dictionary<string, string>();
            d.Add("N", "Returns the Name");
            d.Add("C", "Returns the Colour");
            d.Add("Z", "Returns the Size");
            d.Add("A", "Returns a random adjective.");
            d.Add("3", "Returns random 3 adjectives. Any number above 0 can be used. Comma seperated.");
            d.Add("K", "Returns a random alias");
            d.Add("ID", "Returns this object's ID");
            d.Add("K", "Returns a moniker. If none is found, the name is returned.");
            d.Add("S", "Returns a State adjective.");
            d.Add("V", "Returns an adverb.");
            d.Add("F", "Returns a factual preposition about the object.");
            d.Add("$", "Returns a noun phrase as the subject. 'The' + adjective + moniker.");
            d.Add("numberToText(number)", "Returns a string representation of the number passed.");
            d.Add("adverb(verb)", "Converts an adjectives to an adverb.");
            d.Add("verbToAdverb(verb)", "Converts a verb to an adverb.");
            d.Add("~", "Whatever follows this character is returned with no processing done.");
            d.Add("*", "Converts following commas into periods.");
            d.Add(indefinateArticleCode, "Uses an indefinate article, an adjectives, and a moniker.");
            d.Add("POV", "Returns an integer that represents the point of view of the character.");
            d.Add("DA", "Demonstrative adjective. The pronoun giving is contingient on the pov and if the object is considered as many objects.");
            d.Add("DACLOSE", "This or these");
            d.Add("DAFAR", "Those or that");
            d.Add("OTHER", "Returns the script ID of the object specified in the 'other' textbox.");


            string[] listCodes = { "V", "F", "A", "K", "S" };
            string[] listNames = { "adverbs", "preposition phrases", "subjective adjectives", "moniker", "objective adjectives" };

            for (int x = 0; x < listCodes.Length; x++)
            {
                d.Add(listCodes[x] + "-A(string)", "Calls the value of an associative array in the " + listNames[x] + " list using the string as the key.");
                d.Add(listCodes[x] + "+Digit", "Draws from " + listNames[x] + " a number of times specified by the digit.");
                d.Add(listCodes[x] + "-N", "Draws from the list linearly for the " + listNames[x] + " list, or in other words, in order the list was gathered.");
                d.Add(listCodes[x] + "-A", "Gets the associative value for a random key in the " + listNames[x] + " list. Otherwise it just returns the key. Adverbs to adjectives were in mind, I think.");
                d.Add(listCodes[x] + "-F", "Forces an associative array value to be drawn for the " + listNames[x] + " list. If none is found, an empty string is returned.");
                d.Add(listCodes[x] + "-L", "Returns the last selected key in the " + listNames[x] + " list.");
                d.Add(listCodes[x] + "-R", "Forced Both logic for the " + listNames[x] + " list. I need to reexamine this.");
            }

            d.Add("F-C", "Acts like a simple 'F' command, but adds leading and trailing commas. 'This person (, who is great, )' An extra space between the first comma and the noun phrase will be unavoidable.");

            d.Add("W", "Gets the weight in digit form.");
            d.Add("W-W", "Gets the weight in word form.");
            d.Add("W-WH", "Gets the weight in word form with hyphens.");
            d.Add("EMPTY", "Returns an empty string");
            d.Add("VIDE", "French for empty. Acts the same as the EMPTY command.");
            d.Add("ISMANY", "Returns either 'true' or 'false' as a string.");
            d.Add("PA", "Gets the possessive adjective/determinator.");
            d.Add("RP", "Gets the reflextive pronoun.");
            d.Add("FF", "Returns 'that' with a draw from the fact list.");
            d.Add("FOP", "Returns 'that' with a draw from the fact list.");
            d.Add("FSP", "Returns 'that' with a draw from the fact list.");
            d.Add("G", "Returns the group ID digit.");
            d.Add("AS", "Either a state or adjective is returned. The adjective has a 70% chance over state. If one is empty, the other is returned.");
            d.Add("SA", "Returns a state if there is one. If not, it returns an adjective, else an empty string.");
            d.Add("MOI", "Depending on what is set as 'other', this command returns a determiner phrase that describes the current object how they see it. MOI requires that other has a Trait which follows the following syntax for its script ID: ref" + this.getFullId());
            d.Add("TOI", "Depending on what is set as 'other', this command returns a determiner phrase that describes the current object how they see it. TOI requires that other has a Trait which follows the following syntax for its script ID: ref" + this.getFullId());
            d.Add("MON", "Depending on what is set as 'other', this command returns a possessive determiner phrase that describes the current object how they see it and own it. MON requires that other has a Trait which follows the following syntax for its script ID: ref" + this.getFullId());
            d.Add("YAR", "Does a command 'A-A' and draws from the moniker list. If no moniker exists, the command substitutes that with the 'TYPE' command for Humans, and 'N' command for objects.");
            d.Add("Y", "Draws 'no conflict' from bother adjectives and monikers.");
            d.Add("YSR", "Does a command 'S-A' and draws from the moniker list. If no moniker exists, the command substitutes that with the 'TYPE' command for Humans, and 'N' command for objects.");

            d.Add("WSP", "Returns the parent's SP code.");
            d.Add("WOP", "Returns the parent's OP code.");
            d.Add("WPP", "Returns the parent's PP code.");
            d.Add("WRP", "Returns the parent's RP code.");
            d.Add("WPA", "Returns the parent's PA code.");

            d.Add("WSPP", "Returns the parent's SP code.");
            d.Add("WOPP", "Returns the parent's OP code.");
            d.Add("WPPP", "Returns the parent's PP code.");
            d.Add("WRPP", "Returns the parent's RP code.");
            d.Add("WPAP", "Returns the parent's PA code.");

            return d;
        }

        public void setActorTense(int tense)
        {
            this.tense = tense;
        }

        public int getActorTense()
        {
            return this.tense;
        }

        public void merge(Matter mergedTo, Matter migrant)
        {
            if (mergedTo == null || migrant == null) return;
            mergedTo.AddAdjective(migrant.Adjectives.ToArray());
            mergedTo.addAdverb(migrant.Adverbs.ToArray());
            mergedTo.addState(migrant.States.ToArray());
            mergedTo.AddAlias(migrant.Aliases.ToArray());
            mergedTo.addWho(migrant.Who.ToArray());
        }
    }
}

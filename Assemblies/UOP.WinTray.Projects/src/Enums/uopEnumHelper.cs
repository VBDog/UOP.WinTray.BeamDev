using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using UOP.WinTray.Projects.Enums;

namespace UOP.WinTray.Projects.Utilities
{
    public static class uopEnumHelper
    {

        /// <summary>
        /// Returns the description of the enum value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string Description(this Enum value)
        {
            var attributes = value.GetType().GetField(value.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes.Any())
                return (attributes.First() as DescriptionAttribute).Description;

            // If no description is found, the least we can do is replace underscores with spaces
            // You can add your own custom default formatting logic here
            TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
            return ti.ToTitleCase(ti.ToLower(value.ToString().Replace("_", " ")));
        }

        /// <summary>
        /// Gets all descriptions specified for the enums.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static IEnumerable<Tuple<string, Enum>> GetAllValuesAndDescriptions(Type t)
        {
            if (!t.IsEnum)
                throw new ArgumentException($"{nameof(t)} must be an enum type");

            return Enum.GetValues(t).Cast<Enum>().Where(x => !string.IsNullOrEmpty(x.Description())).Select((e) => new Tuple<string, Enum>(e.Description(), e)).ToList();
        }

        /// <summary>
        /// Gets all descriptions specified for the enums.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static List<string> GetDescriptions(Type t, bool SkipNegatives = true)
        {
            if (!t.IsEnum)
                throw new ArgumentException($"{nameof(t)} must be an enum type");
            Array Vals = Enum.GetValues(t);
            var names = Enum.GetNames(t);
            List<string> _rVal = new List<string>();
            string descr;
            Enum env;
            for (int i = 0; i < Vals.Length; i++)
            {
                env = (Enum)Vals.GetValue(i);
                if (!SkipNegatives || (SkipNegatives && (int)Vals.GetValue(i) >= 0))
                {
                    descr = uopEnums.GetEnumDescription(env);
                    if (string.IsNullOrWhiteSpace(descr)) descr = (string)names.GetValue(i);
                    _rVal.Add(descr);
                }


            }

            return _rVal;

        }

        /// <summary>
        /// Gets all descriptions specified for the enums.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static ObservableCollection<string> GetDescriptionsObs(Type t, bool SkipNegatives = true)
        {
            if (!t.IsEnum)
                throw new ArgumentException($"{nameof(t)} must be an enum type");
            Array Vals = Enum.GetValues(t);
            var names = Enum.GetNames(t);
            ObservableCollection<string> _rVal = new ObservableCollection<string>();
            string descr;
            Enum env;
            for (int i = 0; i < Vals.Length; i++)
            {
                env = (Enum)Vals.GetValue(i);
                if (!SkipNegatives || (SkipNegatives && (int)Vals.GetValue(i) >= 0))
                {
                    descr = uopEnums.GetEnumDescription(env);
                    if (string.IsNullOrWhiteSpace(descr)) descr = (string)names.GetValue(i);
                    _rVal.Add(descr);
                }


            }

            return _rVal;

        }
        /// <summary>
        /// Gets all descriptions specified for the enums.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static string GetDescriptionsList(Type t, bool SkipNegatives = true, char aDelimitor = ',')
        {
            if (!t.IsEnum)
                throw new ArgumentException($"{nameof(t)} must be an enum type");
            Array Vals = Enum.GetValues(t);
            var names = Enum.GetNames(t);
            string _rVal = string.Empty;
            string descr;
            Enum env;
            for (int i = 0; i < Vals.Length; i++)
            {
                env = (Enum)Vals.GetValue(i);
                if (!SkipNegatives || (SkipNegatives && (int)Vals.GetValue(i) >= 0))
                {
                    descr = uopEnums.GetEnumDescription(env);
                    if (string.IsNullOrWhiteSpace(descr)) descr = (string)names.GetValue(i);
                    if (_rVal !=  string.Empty) _rVal += aDelimitor;
                    _rVal += descr;


                }


            }

            return _rVal;

        }
        /// <summary>
        /// Gets the enum value based on the passed description.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static int GetValueByDescription(Type t, string description)
        {
            if (string.IsNullOrWhiteSpace(description)) return 0;
            try
            {
                if (!t.IsEnum) throw new ArgumentException($"{nameof(t)} must be an enum type");
                Array Vals = Enum.GetValues(t);
                var names = Enum.GetNames(t);
                string descr;
                Enum env;
                for (int i = 0; i < Vals.Length; i++)
                {
                    env = (Enum)Vals.GetValue(i);
                    descr = uopEnums.GetEnumDescription(env);
                    if (string.IsNullOrWhiteSpace(descr)) descr = (string)names.GetValue(i);
                    if (string.Compare(descr, description, ignoreCase: true) == 0)
                    {
                        return (int)Vals.GetValue(i);
                    }
                }


                return 0;

            }
            catch (Exception e)
            {
                throw e;
            }

        }

        /// <summary>
        /// Gets all descriptions specified for the enums.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Enum GetMatchingMember(Type t, Int32 aValue, out bool rFound)
        {
            rFound = false;
            if (!t.IsEnum)
                throw new ArgumentException($"{nameof(t)} must be an enum type");
            Array Vals = Enum.GetValues(t);
            Enum _rVal = null;
            Int32 intval;
            Enum env;
            for (int i = 0; i < Vals.Length; i++)
            {
                env = (Enum)Vals.GetValue(i);
                intval = env.GetHashCode();
                if (aValue == intval)
                {
                    _rVal = env;
                    rFound = true;
                    break;
                }
            }

            return _rVal;

        }
    }
}


﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ST.Core.Helpers
{
    public static class EnumHelper
    {
        /// <summary>
        /// Get enumerator description
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetEnumDescription(Enum value)
        {
            var type = value.GetType();
            var name = Enum.GetName(type, value);
            if (name != null)
            {
                var field = type.GetField(name);
                if (field != null)
                {
                    DescriptionAttribute attr =
                        Attribute.GetCustomAttribute(field,
                            typeof(DescriptionAttribute)) as DescriptionAttribute;
                    return attr?.Description;
                }
            }

            return null;
        }
    }
}

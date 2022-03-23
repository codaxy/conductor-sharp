using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace XgsPon.Workflows.Engine.Util
{
    public class ObjectValidator
    {
        public static void Validate(object obj)
        {
            var context = new ValidationContext(obj);

            var validationResults = new List<ValidationResult>();

            if (!Validator.TryValidateObject(obj, context, validationResults, true))
                throw new ValidationException(
                    string.Join(
                        ",",
                        validationResults.Select(a => a.ErrorMessage.Replace('.', ' '))
                    )
                );
        }
    }
}

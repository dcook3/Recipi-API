using System.ComponentModel.DataAnnotations;

namespace Recipi_API.Models.Data_Models
{
    public class ValidateListAttribute<T>: ValidationAttribute
    {
        protected override ValidationResult IsValid(object list, ValidationContext validationContext)
        {
            List<ValidationResult> results;
            ValidationContext context;
            
            foreach (var value in (ICollection<T>)list) { 
                results = new List<ValidationResult>();
                context = new ValidationContext(value, null,null);

                Validator.TryValidateObject(value, context, results, true);

                if (results.Count != 0)
                {
                    var compositeResults = new CompositeValidationResult(String.Format("Validation for {0} failed!", validationContext.DisplayName));
                    results.ForEach(compositeResults.AddResult);

                    return compositeResults;
                }
            }
            return ValidationResult.Success;
        }
    }

    public class CompositeValidationResult : ValidationResult
    {
        private readonly List<ValidationResult> _results = new List<ValidationResult>();

        public IEnumerable<ValidationResult> Results
        {
            get
            {
                return _results;
            }
        }

        public CompositeValidationResult(string errorMessage) : base(errorMessage) { }
        public CompositeValidationResult(string errorMessage, IEnumerable<string> memberNames) : base(errorMessage, memberNames) { }
        protected CompositeValidationResult(ValidationResult validationResult) : base(validationResult) { }

        public void AddResult(ValidationResult validationResult)
        {
            _results.Add(validationResult);
        }
    }
}

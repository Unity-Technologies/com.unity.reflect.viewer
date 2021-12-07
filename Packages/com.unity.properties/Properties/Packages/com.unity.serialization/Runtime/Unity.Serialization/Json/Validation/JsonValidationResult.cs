namespace Unity.Serialization.Json
{
    struct JsonValidationResult
    {
        /// <summary>
        /// The validation type that was used.
        /// </summary>
        public JsonValidationType ValidationType;
        
        /// <summary>
        /// The type that was expected by the validator.
        /// </summary>
        public JsonType ExpectedType;

        /// <summary>
        /// The type that the validator stopped at.
        /// </summary>
        public JsonType ActualType;

        /// <summary>
        /// The character that the validator stopped at.
        /// </summary>
        public char Char;

        /// <summary>
        /// The line the validator stopped at.
        /// </summary>
        public int LineCount;

        /// <summary>
        /// The char (on the line) the validator stopped at.
        /// </summary>
        public int CharCount;

        public bool IsValid()
        {
            return (ActualType & ExpectedType) == ActualType;
        }

        public override string ToString()
        {
            var actualChar = Char == '\0' ? "\\0" : Char.ToString();
            var isValid = IsValid() ? "valid" : "invalid";
            return $"Input json was {isValid}. {nameof(ExpectedType)}=[{ExpectedType}] {nameof(ActualType)}=[{ActualType}] ActualChar=['{actualChar}'] at Line=[{LineCount}] at Character=[{CharCount}]";
        }
    }
}
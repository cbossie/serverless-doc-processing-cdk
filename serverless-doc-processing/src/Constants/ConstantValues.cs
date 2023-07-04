using System;

namespace Constants
{
    public static class ConstantValues
    {
        public const string QUERY_TAG_KEY = "QUERY_TAG_KEY";
        public const string QUERY_TAG = "Queries";

        public const string ID_TAG_KEY = "ID_TAG_KEY";
        public const string ID_TAG = "Id";

        public const string ENVIRONMENT_NAME_VARIABLE = "ENVIRONMENT_NAME";


        public const string TEXTRACT_TOPIC_KEY = "TEXTRACT_TOPIC";
        public const string TEXTRACT_ROLE_KEY = "TEXTRACT_ROLE";
        public const string TEXTRACT_BUCKET_KEY = "TEXTRACT_BUCKET_KEY";
        public const string TEXTRACT_OUTPUT_KEY_KEY = "TEXTRACT_BUCKET_KEY";
        public const string TEXTRACT_OUTPUT_KEY = "results";
    }

    public static class ResourceNames
    {
        public const string PROCESS_DATA_TABLE = "ProcessData";
        public const string QUERY_DATA_TABLE = "QueryData";
    }
    public static class DefaultValues
    {
        public const int TEXTRACT_STEP_TIME_OUT = 300;
    }

}

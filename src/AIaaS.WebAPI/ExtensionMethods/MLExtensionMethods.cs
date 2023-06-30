using Microsoft.ML.Data;

namespace AIaaS.WebAPI.ExtensionMethods
{
    public static class MLExtensionMethods
    {
        public static Type ToRawType(this DataViewType dataViewType)
        {
            return dataViewType is TextDataViewType ?
                typeof(string) :
                dataViewType.RawType;
        }
    }
}

using Microsoft.ML;
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

        public static IEstimator<ITransformer> AppendEstimator(this IEstimator<ITransformer> estimatorChain, IEstimator<ITransformer> estimator)
        {
            return estimatorChain is not null ?
                 estimatorChain.Append(estimator) :
                 estimator;
        }
    }
}

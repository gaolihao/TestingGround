namespace MyApi;

using MyApi.Contract;


public interface ISynchronizedFeatureRepository
{

    //public IDictionary<int, FeatureSource> All;

    public FeatureSource GetPathAndQueue(int processId);

    public FeatureSource GetPathAndQueue(int processId, string filePath);

    public void Delete(int processId);
}

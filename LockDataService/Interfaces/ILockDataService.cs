using System.ServiceModel;

namespace LockDataService.Interfaces
{
    [ServiceContract]
    public interface ILockDataService
    {
        
        [OperationContract]
        void setId(int id);

    }
}
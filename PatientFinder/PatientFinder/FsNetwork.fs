namespace PatientFinder.Network

open System

module FsNetwork =
   // let context = new MedicalService.MedicalServiceClient(new BasicHttpBinding(MaxReceivedMessageSize =2147483647L), new EndpointAddress("http://sager:9003/MedicalServiceHost.svc"))

    let context = new MedicalService.MedicalServiceClient()


    (*//to read database
    let getAllPatientName : ViewName[] = 
        Async.RunSynchronously (FsNetwork.GetAllPatientNamesAsync()) *)

    /// Get all the patient names from the patient file and appointment book with number of missed appointments and
    /// chart number if available.
    let GetAllPatientNamesAsync() =
        async {
                let data = context.GetAllPatientNamesAsync() 
                return! Async.AwaitTask data
             }
       
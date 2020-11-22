namespace PatientFinder.Network

open System

module FsNetwork =
   // let context = new MedicalService.MedicalServiceClient(new BasicHttpBinding(MaxReceivedMessageSize =2147483647L), new EndpointAddress("http://sager:9003/MedicalServiceHost.svc"))

    let context = new MedicalService.MedicalServiceClient()

   
    /// Get the office schedule for the tableDate.
    let GetScheduleAsync (tableDate : DateTime) : Async<MedicalService.Visit[]> =
        async {
            let data = context.GetOfficeScheduleAsync(tableDate) 
            return! Async.AwaitTask data               
        }


    (*//to read database
    let getAllAppointmentsForDate : ViewName[] = 
        Async.RunSynchronously (FsNetwork.GetAllPatientNamesAsync()) *)

    /// Get all the patient names from the patient file and appointment book with number of missed appointments and
    /// chart number if available.
    let GetAllPatientNamesAsync() =
        async {
            let data = context.GetAllPatientNamesAsync() 
            return! Async.AwaitTask data
            }
       
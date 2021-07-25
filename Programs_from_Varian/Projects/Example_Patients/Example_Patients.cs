using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace Example_Patients
{
  class Program
  {
    [STAThread] // Do not remove this attribute, otherwise the script will not work
    static void Main(string[] args)
    {
      try
      {
        Console.WriteLine("Logging in...");
        using (Application app = Application.CreateApplication(null, null))
        {
          Console.WriteLine("Running script...");
          Execute(app);
        }
      }
      catch (Exception exception)
      {
        Console.WriteLine("Exception was thrown:" + exception.Message);
      }

      Console.WriteLine("Execution finished. Press any key to exit.");
      Console.ReadKey();
    }

    static void Execute(Application app)
    {
      // Iterate through all patients

      int counter = 0;

      foreach (var patientSummary in app.PatientSummaries)
      {
        // Stop after when a few records have been found
        if (counter > 10)
          break;

        // For the test purpose we are interesting in patient records created during the last 6 months
        DateTime startDate = DateTime.Now - TimeSpan.FromDays(30 * 6);

        if (patientSummary.CreationDateTime > startDate)
        {
          // Retrieve patient information
          Patient patient = app.OpenPatient(patientSummary);
          if (patient == null)
            throw new ApplicationException("Cannot open patient " + patientSummary.Id);

          // Iterate through all patient courses...
          foreach (var course in patient.Courses)
          {
            // ... and plans
            foreach (var planSetup in course.PlanSetups)
            {
              try
              {
                // For the test purpose we will look into approved plans with calculated 3D dose only...
                PlanSetupApprovalStatus status = planSetup.ApprovalStatus;
                if ((status == PlanSetupApprovalStatus.PlanningApproved || status == PlanSetupApprovalStatus.TreatmentApproved) && planSetup.Dose != null)
                {
                  // ... select dose values to be in absolute unit
                  planSetup.DoseValuePresentation = DoseValuePresentation.Absolute;

                  // ... and display information about max dose
                  string message = string.Format("Patient: {0}, Course: {1}, Plan: {2}, Max dose: {3}", patient.Id, course.Id, planSetup.Id, planSetup.Dose.DoseMax3D.ToString());
                  Console.WriteLine(message);
                  counter = counter + 1;
                }
              }
              catch (ApplicationException exception)
              {
                // In case of any error we will display some useful information...
                string errorMsg = string.Format("Exception was thrown. Patient Id: {0}, Course: {1}, Exception: {2}", patient.Id, course.Id, exception.Message);
                Console.WriteLine(errorMsg);
                // ... and then move to the next patient
                continue;
              }
            }
          }
          // Close the current patient, otherwise we will not be able to open another patient
          app.ClosePatient();
        }
      }
    }
  }
}

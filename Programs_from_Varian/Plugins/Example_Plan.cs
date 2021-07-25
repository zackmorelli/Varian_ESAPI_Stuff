using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

// Do not change namespace and class name
// otherwise Eclipse will not be able to run the script.
namespace VMS.TPS
{
  class Script
  {
    public Script()
    {
    }

    // Second parameter is commented out because we do not need a window for this script
    public void Execute(ScriptContext context /*, System.Windows.Window window */)
    {
      // Retrieve the count of plans displayed in Scope Window
      int scopePlanCount = context.PlansInScope.Count();
      if (scopePlanCount == 0)
      {
        MessageBox.Show("Scope Window does not contain any plans.");
        return;
      }

      // Retrieve names for different types of plans
      List<string> externalPlanIds = new List<string>();
      List<string> brachyPlanIds = new List<string>();
      List<string> protonPlanIds = new List<string>();
      foreach (var ps in context.PlansInScope)
      {
        if (ps is BrachyPlanSetup)
        {
          brachyPlanIds.Add(ps.Id);
        }
        else if (ps is IonPlanSetup)
        {
          protonPlanIds.Add(ps.Id);
        }
        else
        {
          externalPlanIds.Add(ps.Id);
        }
      }      
      
      // Construct output message
      string message = string.Format("Hello {0}, the number of plans in Scope Window is {1}.",
        context.CurrentUser.Name,
        scopePlanCount);
      if (externalPlanIds.Count > 0)
      {
        message += string.Format("\nPlan(s) {0} are external beam plans.", string.Join(", ", externalPlanIds));
      }
      if (brachyPlanIds.Count > 0)
      {
        message += string.Format("\nPlan(s) {0} are brachytherapy plans.", string.Join(", ", brachyPlanIds));
      }
      if (protonPlanIds.Count > 0)
      {
        message += string.Format("\nPlan(s) {0} are proton plans.", string.Join(", ", protonPlanIds));
      }

      // Display additional information. Use the active plan if available.
      PlanSetup plan = context.PlanSetup != null ? context.PlanSetup : context.PlansInScope.ElementAt(0);
      message += string.Format("\n\nAdditional details for plan {0}:", plan.Id);

      // Access the structure set of the plan
      if (plan.StructureSet != null)
      {
        Image image = plan.StructureSet.Image;
        var structures = plan.StructureSet.Structures;
        message += string.Format("\n* Image ID: {0}", image.Id);
        message += string.Format("\n* Size of the Structure Set associated with the plan: {0}.", structures.Count());
      }
      Fractionation fractionation = plan.UniqueFractionation;
      message += string.Format("\n* Number of Fractions: {0}.", fractionation.NumberOfFractions);

      // Handle brachytherapy plans separately from external beam plans
      if (plan is BrachyPlanSetup)
      {
        BrachyPlanSetup brachyPlan = (BrachyPlanSetup)plan;
        var catheters = brachyPlan.Catheters;
        var seedCollections = brachyPlan.SeedCollections;
        message += string.Format("\n* Number of Catheters: {0}.", catheters.Count());
        message += string.Format("\n* Number of Seed Collections: {0}.", seedCollections.Count());
      }
      else
      {
        var beams = plan.Beams;
        message += string.Format("\n* Number of Beams: {0}.", beams.Count());
      }
      if (plan is IonPlanSetup)
      {
        IonPlanSetup ionPlan = plan as IonPlanSetup;
        IonBeam beam = ionPlan.IonBeams.FirstOrDefault();
        if (beam != null)
        {
          message += string.Format("\n* Number of Lateral Spreaders in first beam: {0}.", beam.LateralSpreadingDevices.Count());
          message += string.Format("\n* Number of Range Modulators in first beam: {0}.", beam.RangeModulators.Count());
          message += string.Format("\n* Number of Range Shifters in first beam: {0}.", beam.RangeShifters.Count());
        }
      }
      
      MessageBox.Show(message);
    }
  }
}

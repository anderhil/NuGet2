using System;
using System.Collections.Generic;
using System.Linq;
using EnvDTE;

namespace NuGet.VisualStudio {

    public static class SolutionExtensions {
        /// <summary>
        /// Get the list of all supported projects in the current solution. This method
        /// recursively iterates through all projects.
        /// </summary>
        public static IEnumerable<Project> GetAllProjects(this Solution solution) {
            if (solution == null || !solution.IsOpen) {
                yield break;
            }

            var projects = new Stack<Project>();
            foreach (Project project in solution.Projects) {
                projects.Push(project);
            }

            while (projects.Any()) {
                Project project = projects.Pop();

                if (project.IsSupported()) {
                    yield return project;
                }
                else if (project.IsExplicitlyUnsupported()) {
                    // do not drill down further if this project is explicitly unsupported, e.g. LightSwitch projects
                    continue;
                }

                ProjectItems projectItems = null;
                try {
                    // bug 1138: Oracle Database Project doesn't implement the ProjectItems property
                    projectItems = project.ProjectItems;
                }
                catch (NotImplementedException) {
                    continue;
                }

                // ProjectItems property can be null if the project is unloaded
                if (projectItems != null) {
                    foreach (ProjectItem projectItem in projectItems) {
                        if (projectItem.SubProject != null) {
                            projects.Push(projectItem.SubProject);
                        }
                    }
                }
            }
        }

        public static string GetName(this Solution solution) {
            return solution.Properties.Item("Name").Value;
        }
    }
}

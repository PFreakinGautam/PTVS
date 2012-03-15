﻿/* ****************************************************************************
 *
 * Copyright (c) Steve Dower (Zooba).
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.TC.TestHostAdapters;
using TestUtilities;
using System.IO;
using EnvDTE;
using EnvDTE80;

namespace AnalysisTest.ProjectSystem {
    [TestClass]
    [DeploymentItem(@"Python.VS.TestData\", "Python.VS.TestData")]
    public class ProjectHomeTests {
        [TestCleanup]
        public void MyTestCleanup() {
            VsIdeTestHostContext.Dte.Solution.Close(false);
        }

        [TestMethod, Priority(2), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void LoadRelativeProjects() {
            string fullPath = Path.GetFullPath(@"Python.VS.TestData\ProjectHomeProjects.sln");
            Assert.IsTrue(File.Exists(fullPath), "Can't find project file");
            VsIdeTestHostContext.Dte.Solution.Open(fullPath);

            try {
                Assert.IsTrue(VsIdeTestHostContext.Dte.Solution.IsOpen, "The solution is not open");
                Assert.IsTrue(VsIdeTestHostContext.Dte.Solution.Projects.Count == 9, String.Format("Loading project resulted in wrong number of loaded projects, expected 9, received {0}", VsIdeTestHostContext.Dte.Solution.Projects.Count));

                foreach (var project in VsIdeTestHostContext.Dte.Solution.Projects.OfType<Project>()) {
                    var name = Path.GetFileName(project.FileName);
                    if (name.StartsWith("ProjectA")) {
                        // Should have ProgramA.py, Subfolder\ProgramB.py and Subfolder\Subsubfolder\ProgramC.py
                        var programA = project.ProjectItems.Item("ProgramA.py");
                        Assert.IsNotNull(programA);

                        var subfolder = project.ProjectItems.Item("Subfolder");
                        var programB = subfolder.ProjectItems.Item("ProgramB.py");
                        Assert.IsNotNull(programB);

                        var subsubfolder = subfolder.ProjectItems.Item("Subsubfolder");
                        var programC = subsubfolder.ProjectItems.Item("ProgramC.py");
                        Assert.IsNotNull(programC);
                    } else if (name.StartsWith("ProjectB")) {
                        // Should have ProgramB.py and Subsubfolder\ProgramC.py
                        var programB = project.ProjectItems.Item("ProgramB.py");
                        Assert.IsNotNull(programB);

                        var subsubfolder = project.ProjectItems.Item("Subsubfolder");
                        var programC = subsubfolder.ProjectItems.Item("ProgramC.py");
                        Assert.IsNotNull(programC);
                    } else if (name.StartsWith("ProjectSln")) {
                        // Should have ProjectHomeProjects\ProgramA.py, 
                        // ProjectHomeProjects\Subfolder\ProgramB.py and
                        // ProjectHomeProjects\Subfolder\Subsubfolder\ProgramC.py
                        var projectHome = project.ProjectItems.Item("ProjectHomeProjects");
                        var programA = projectHome.ProjectItems.Item("ProgramA.py");
                        Assert.IsNotNull(programA);

                        var subfolder = projectHome.ProjectItems.Item("Subfolder");
                        var programB = subfolder.ProjectItems.Item("ProgramB.py");
                        Assert.IsNotNull(programB);

                        var subsubfolder = subfolder.ProjectItems.Item("Subsubfolder");
                        var programC = subsubfolder.ProjectItems.Item("ProgramC.py");
                        Assert.IsNotNull(programC);
                    } else {
                        Assert.Fail("Wrong project file name", name);
                    }
                }
            } finally {
                VsIdeTestHostContext.Dte.Solution.Close();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        [TestMethod, Priority(2), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void AddDeleteItem() {
            string fullPath = Path.GetFullPath(@"Python.VS.TestData\ProjectHomeSingleProject.sln");
            Assert.IsTrue(File.Exists(fullPath), "Can't find project file");
            VsIdeTestHostContext.Dte.Solution.Open(fullPath);

            try {
                var project = VsIdeTestHostContext.Dte.Solution.Projects.OfType<Project>().Single();
                Assert.AreEqual("ProjectSingle.pyproj", Path.GetFileName(project.FileName));
                
                project.ProjectItems.AddFromTemplate(((Solution2)VsIdeTestHostContext.Dte.Solution).GetProjectItemTemplate("PyClass.zip", "pyproj"), "TemplateItem.py");

                var newItem = project.ProjectItems.Item("TemplateItem.py");
                Assert.IsNotNull(newItem);
                Assert.AreEqual(false, project.Saved);
                project.Save();
                Assert.AreEqual(true, project.Saved);
                Assert.IsTrue(File.Exists(Path.GetFullPath(@"Python.VS.TestData\ProjectHomeProjects\TemplateItem.py")));

                newItem.Delete();
                Assert.AreEqual(false, project.Saved);
                project.Save();
                Assert.AreEqual(true, project.Saved);
                Assert.IsFalse(File.Exists(Path.GetFullPath(@"Python.VS.TestData\ProjectHomeProjects\TemplateItem.py")));
            } finally {
                VsIdeTestHostContext.Dte.Solution.Close();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        [TestMethod, Priority(2), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void AddDeleteItem2() {
            string fullPath = Path.GetFullPath(@"Python.VS.TestData\ProjectHomeSingleProject.sln");
            Assert.IsTrue(File.Exists(fullPath), "Can't find project file");
            VsIdeTestHostContext.Dte.Solution.Open(fullPath);

            try {
                var project = VsIdeTestHostContext.Dte.Solution.Projects.OfType<Project>().Single();
                var folder = project.ProjectItems.Item("Subfolder");

                Assert.AreEqual("ProjectSingle.pyproj", Path.GetFileName(project.FileName));

                folder.ProjectItems.AddFromTemplate(((Solution2)VsIdeTestHostContext.Dte.Solution).GetProjectItemTemplate("PyClass.zip", "pyproj"), "TemplateItem.py");

                var newItem = folder.ProjectItems.Item("TemplateItem.py");
                Assert.IsNotNull(newItem);
                Assert.AreEqual(false, project.Saved);
                project.Save();
                Assert.AreEqual(true, project.Saved);
                Assert.IsTrue(File.Exists(Path.GetFullPath(@"Python.VS.TestData\ProjectHomeProjects\Subfolder\TemplateItem.py")));

                newItem.Delete();
                Assert.AreEqual(false, project.Saved);
                project.Save();
                Assert.AreEqual(true, project.Saved);
                Assert.IsFalse(File.Exists(Path.GetFullPath(@"Python.VS.TestData\ProjectHomeProjects\Subfolder\TemplateItem.py")));
            } finally {
                VsIdeTestHostContext.Dte.Solution.Close();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        [TestMethod, Priority(2), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void AddDeleteFolder() {
            string fullPath = Path.GetFullPath(@"Python.VS.TestData\ProjectHomeSingleProject.sln");
            Assert.IsTrue(File.Exists(fullPath), "Can't find project file");
            VsIdeTestHostContext.Dte.Solution.Open(fullPath);

            try {
                var project = VsIdeTestHostContext.Dte.Solution.Projects.OfType<Project>().Single();
                Assert.AreEqual("ProjectSingle.pyproj", Path.GetFileName(project.FileName));

                project.ProjectItems.AddFolder("NewFolder");

                var newFolder = project.ProjectItems.Item("NewFolder");
                Assert.IsNotNull(newFolder);
                Assert.AreEqual(Path.GetFullPath(@"Python.VS.TestData\ProjectHomeProjects\NewFolder\"), newFolder.Properties.Item("FullPath").Value);
                newFolder.Delete();
            } finally {
                VsIdeTestHostContext.Dte.Solution.Close();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        [TestMethod, Priority(2), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void AddDeleteSubfolder() {
            string fullPath = Path.GetFullPath(@"Python.VS.TestData\ProjectHomeSingleProject.sln");
            Assert.IsTrue(File.Exists(fullPath), "Can't find project file");
            VsIdeTestHostContext.Dte.Solution.Open(fullPath);

            try {
                var project = VsIdeTestHostContext.Dte.Solution.Projects.OfType<Project>().Single();
                var folder = project.ProjectItems.Item("Subfolder");

                Assert.AreEqual("ProjectSingle.pyproj", Path.GetFileName(project.FileName));

                folder.ProjectItems.AddFolder("NewFolder");

                var newFolder = folder.ProjectItems.Item("NewFolder");
                Assert.IsNotNull(newFolder);
                Assert.AreEqual(Path.GetFullPath(@"Python.VS.TestData\ProjectHomeProjects\Subfolder\NewFolder\"), newFolder.Properties.Item("FullPath").Value);
                newFolder.Delete();
            } finally {
                VsIdeTestHostContext.Dte.Solution.Close();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        [TestMethod, Priority(2), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void SaveProjectAs() {
            try {
                var project = DebugProject.OpenProject(@"Python.VS.TestData\HelloWorld.sln");

                project.SaveAs(Path.GetFullPath(@"Python.VS.TestData\ProjectHomeProjects\TempFile.pyproj"));

                Assert.AreEqual(Path.GetFullPath(@"Python.VS.TestData\HelloWorld\"),
                    ((Microsoft.PythonTools.Project.Automation.OAProject)project).Project.ProjectHome);

                project.Delete();
            } finally {
                VsIdeTestHostContext.Dte.Solution.Close();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        // TODO: Drag-drop tests

        // TODO: Cut-copy tests
    }
}

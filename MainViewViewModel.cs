using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Prism.Commands;
using RevitAPIUILibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAPIUI6._3
{
    public class MainViewViewModel
    {
        private ExternalCommandData _commandData;

        public DelegateCommand SaveCommand { get; }
        public List<FamilySymbol> FurnTypes { get; } = new List<FamilySymbol>();
        public List<Level> FurnLevels { get; } = new List<Level>();
        public FamilySymbol SelectedFurnType { get; set; }
        public Level SelectedLevel { get; set; }
        public List<XYZ> Points { get; } = new List<XYZ>();
        public List<string> ElementCount { get; } = new List<string>();
        public string SelectedCount { get; set; }




        public MainViewViewModel(ExternalCommandData commandData)
        {
            _commandData = commandData;
            SaveCommand = new DelegateCommand(OnSaveCommand);
            FurnTypes = FurnitureUtils.GetFurnitureTypes(commandData);
            FurnLevels = LevelsUtils.GetLevels(commandData);
            Points = SelectionUtils.GetPoints(_commandData, "Выберите точки", ObjectSnapTypes.Endpoints);
            ElementCount = SelectionUtils.GetCount(commandData);

        }

        private void OnSaveCommand()
        {
            //RaiseHideRequest();

            UIApplication uiapp = _commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            if (Points.Count < 2 || SelectedFurnType == null || SelectedLevel == null)
                return;

            var num = Convert.ToInt32(SelectedCount);
            XYZ pStart = Points[0];
            XYZ pEnd = Points[1];
            var v = new XYZ(pEnd.X - pStart.X, pEnd.Y - pStart.Y, pEnd.Z - pStart.Z);
            var lenghtFull = SelectionUtils.GetLenght(_commandData, pStart, pEnd);
            var lenCut = lenghtFull / num;
            var curves = new List<Curve>();
            for (int i = 0; i <= num; i++)
            {
                if (i == 0)
                    continue;

                var currentPoint = pEnd;
                var prevPoint = new XYZ(pEnd.X - v.X * (lenCut/lenghtFull), pEnd.Y - v.Y * (lenCut/lenghtFull), pEnd.Z - v.Z * (lenCut/lenghtFull));             

                Curve curve = Line.CreateBound(prevPoint, currentPoint);
                curves.Add(curve);

                pEnd = prevPoint;
            }

                foreach (var curve in curves)
                {
                    XYZ p1 = curve.GetEndPoint(0);
                    XYZ p2 = curve.GetEndPoint(1);
                    XYZ pMid = (p1 + p2) / 2;
                    FamilyInstanceUtils.CreateFamilyInstance(_commandData, SelectedFurnType, pMid, SelectedLevel);
                }

            RaiseCloseRequest();
            //RaiseShowRequest();
        }

        public event EventHandler CloseRequest;
        private void RaiseCloseRequest()
        {
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }

        //public event EventHandler HideRequest;
        //private void RaiseHideRequest()
        //{
        //    HideRequest?.Invoke(this, EventArgs.Empty);
        //}

        //public event EventHandler ShowRequest;
        //private void RaiseShowRequest()
        //{
        //    ShowRequest?.Invoke(this, EventArgs.Empty);
        //}
    }
}


// This demo application was started from
//    WPF 3D Primer by Dario Solera
//    http://www.codeproject.com/Articles/23332/WPF-3D-Primer
//       under the CPOL license 
//       as a derivative work
//    which I then extended for my own purposes
//    - Paul Schrum 6 June 2013
//    

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Windows.Media.Media3D;
using System.Media;
using System.Threading;
using System.Windows.Threading;
using System.Diagnostics;
using DataTicker3D;

namespace DataTicker3dViz
{
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window
   {
      private GeometryModel3D mGeometry;
      private bool mDown;
      private Point mLastPos;
      private Point startMousePosition;
      private int mouseDownCount;
      DispatcherTimer dispatcherTimer = new DispatcherTimer();
      //bool timerIsRegistered;
      USGS_LakeLevelTxtFileReader USGS_LakeLevelTxtFileReader = null;
      private int theDirectionalLight = -1;
      private Dictionary<System.Windows.Input.Key, bool> keyIsDown;
      private Vector3D viewpointVelocity { get; set; }
      private Double viewpointForwardSpeed { get; set; }
      private Double viewpointRotationSpeedAboutWorldZ { get; set; }
      private Double viewpointUpAngleRotationSpeed { get; set; }
      private Point3D cameraOriginalPosition { get; set; }
      private Vector3D totalCameraMoveVector { get; set; }
      private bool printDiagnostics { get; set; }

      public MainWindow()
      {
         InitializeComponent();

         //testRotateVector();
         viewpointVelocity = new Vector3D(0, 0, 0);
         viewpointRotationSpeedAboutWorldZ = 0.0;
         viewpointUpAngleRotationSpeed = 0.25;
         hydrateKeyIsDownDictionary();
         this.camera.Position = new Point3D(15, 20, 35);
         this.camera.FieldOfView = 50;
         this.camera.LookDirection = new Vector3D(-10, -15, -35);
         this.camera.UpDirection = new Vector3D(0, 1, 0);
         cameraOriginalPosition = new Point3D(this.camera.Position.X,
            this.camera.Position.Y, this.camera.Position.Z);

         int count = -1;
         foreach (var aThing in this.group.Children)
         {
            count++;
            if (aThing is DirectionalLight)
            {
               theDirectionalLight = count;
               break;
            }
         }

         testRotateVector();
         startMousePosition = new Point(0, 0);
         mouseDownCount = 0;
         dispatcherTimer.Interval = new TimeSpan(1000000L);
         dispatcherTimer.Tick += keepRotatingScreen;
         dispatcherTimer.Start();

         makeAtestObject();
         openDataFile();

      }

      private void hydrateKeyIsDownDictionary()
      {
         keyIsDown = new Dictionary<System.Windows.Input.Key, bool>();
         keyIsDown.Add(System.Windows.Input.Key.R, false);        // reset view to original position
         keyIsDown.Add(System.Windows.Input.Key.A, false);        // accelerate forward
         keyIsDown.Add(System.Windows.Input.Key.Z, false);        // accelerate backward
         keyIsDown.Add(System.Windows.Input.Key.Space, false);    // reset acceleration to 0
         keyIsDown.Add(System.Windows.Input.Key.Up, false);       // increase up angle
         keyIsDown.Add(System.Windows.Input.Key.Down, false);     // decrease up angle (dive)
         keyIsDown.Add(System.Windows.Input.Key.Left, false);     // rotate left
         keyIsDown.Add(System.Windows.Input.Key.Right, false);    // rotate right
         keyIsDown.Add(System.Windows.Input.Key.NumPad4, false);  // slew -X
         keyIsDown.Add(System.Windows.Input.Key.NumPad6, false);  // slew +X
         keyIsDown.Add(System.Windows.Input.Key.NumPad8, false);  // slew Up (+Y, world Z)
         keyIsDown.Add(System.Windows.Input.Key.NumPad2, false);  // slew Down (-Y, world Z)
         keyIsDown.Add(System.Windows.Input.Key.Divide, false);  // slew Left (+Z, world Y)
         keyIsDown.Add(System.Windows.Input.Key.NumPad0, false);   // slew Right (-Z, world Y)
         
      }


      private void makeAtestObject()
      {
         MeshGeometry3D mesh = new MeshGeometry3D();
         //mesh.Positions.Add(new Point3D(-3, 0, 0));
         //mesh.Positions.Add(new Point3D(-1.25, 2.5, -1));
         //mesh.Positions.Add(new Point3D(-1.5, 0, -2));

         mesh.Positions.Add(new Point3D(0, 0, 0));
         mesh.Positions.Add(new Point3D(-8, 0, -1));
         mesh.Positions.Add(new Point3D(-8, 0, 1));

         //mesh.Positions.Add(new Point3D(0, 0, -5));
         //mesh.Positions.Add(new Point3D(0, 0, 5));
         //mesh.Positions.Add(new Point3D(1, 0.13, -5));
         //mesh.Positions.Add(new Point3D(1, 0.13, 5));

         //mesh.Positions.Add(new Point3D(-0.5, 0, -5));
         //mesh.Positions.Add(new Point3D(-0.4, 0, 5));
         //mesh.Positions.Add(new Point3D(0.2, -0.13, -5));
         //mesh.Positions.Add(new Point3D(0.2, -0.13, 5));

         //mesh.TriangleIndices = new Int32Collection(new int[] { 0, 1, 2, 3, 1, 2 });
         mesh.TriangleIndices = new Int32Collection(new int[] { 0, 1, 2 });

         GeometryModel3D geomod = new GeometryModel3D();
         geomod.Geometry = mesh;
         geomod.Material = new DiffuseMaterial(Brushes.Cyan);
         geomod.BackMaterial = new DiffuseMaterial(Brushes.Red);
         //geomod.Transform = new Transform3DGroup();

         //ModelVisual3D modvis = new ModelVisual3D();
         //modvis.Content = geomod;
         this.group.Children.Add(geomod);

         geomod = new GeometryModel3D();
         mesh = new MeshGeometry3D();
         geomod.Geometry = mesh;

         mesh.Positions.Add(new Point3D(0, 0, 0));
         mesh.Positions.Add(new Point3D(-5, 1, 0));
         mesh.Positions.Add(new Point3D(-5, 0, 0));

         mesh.TriangleIndices = new Int32Collection(new int[] { 0, 1, 2 });

         geomod.Geometry = mesh;

         DiffuseMaterial dm = new DiffuseMaterial(Brushes.Purple);
         dm.Color = Color.FromArgb(126, 127, 0, 127);
         geomod.Material = dm;
         geomod.BackMaterial = new DiffuseMaterial(Brushes.Orange);
         //geomod.Transform = new Transform3DGroup();

         //ModelVisual3D modvis = new ModelVisual3D();
         //modvis.Content = geomod;
         this.group.Children.Add(geomod);
      }

      private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
      {
         //camera.Position = new Point3D(camera.Position.X, camera.Position.Y, 
         //camera.Position.Z - e.Delta / 250D);
         Double fovMultiplier = e.Delta > 0 ? 0.8 : -1 / 0.8;
         camera.FieldOfView *= fovMultiplier * e.Delta / 30D;
         Debug.Print(camera.FieldOfView.ToString());
      }

      private void Button_Click(object sender, RoutedEventArgs e)
      {
         camera.Position = new Point3D(camera.Position.X, camera.Position.Y, 5);
         mGeometry.Transform = new Transform3DGroup();
      }

      private Double lightX = -8;
      private void keepRotatingScreen(object sender, EventArgs e)
      {
         processKeyPresses();
         if (this.mDown == false)
         {
            //Debug.Print("- Mouse button is not down.");
            Double addValue = 0.25;
            if (lightX > 8.0) lightX = -8.0;
            lightX += addValue;
            this.group.Children[theDirectionalLight] = new DirectionalLight(Colors.Bisque,
               new Vector3D(lightX, -5, -7)); /* */
            //Debug.Print(lightX.ToString());
            return;
         }
         processMouseNavigation(true);
         //Debug.Print(". Now it is down.");

      }

      private void Grid_MouseMove(object sender, MouseEventArgs e)
      {
         if (e.LeftButton == MouseButtonState.Pressed)
         {
            this.mDown = true;
         }
         else
         {
            this.mDown = false;
         }
         //processMouseNavigation(e.LeftButton == MouseButtonState.Pressed);
      }

      private Double getMagnitude(Double x, Double y)
      {
         return Math.Sqrt(x * x + y * y);
      }

      private void processMouseNavigation(bool leftButtonIsPressed)
      //private void processMouseNavigation(object sender, MouseEventArgs e)
      {
         if (true == this.mDown)
         {
            Point pos = Mouse.GetPosition(viewport);
            if (mouseDownCount > 0)
            {
               //Thread.Sleep(25);


               double screenDeltaX = pos.X - startMousePosition.X;
               double screenDeltaY = pos.Y - startMousePosition.Y;
               Double zBearing = Math.Atan2(camera.LookDirection.Y, camera.LookDirection.X);
               Double magnitude = getMagnitude(camera.LookDirection.X, camera.LookDirection.Y);
               //Debug.Print(camera.LookDirection.X.ToString());

               zBearing += screenDeltaX / 500;
               Double newXcomponent = magnitude * Math.Cos(zBearing);
               Double newYcomponent = magnitude * Math.Sin(zBearing);
               //Debug.Print(screenDeltaX.ToString() + "  " + zBearing.ToString() + "  " + "");
               Debug.Print("Before: " + camera.LookDirection.ToString());
               camera.LookDirection = new Vector3D(newXcomponent,
                  newYcomponent, camera.LookDirection.Z);
               Debug.Print("After: " + camera.LookDirection.ToString());
               Debug.Print("");

            }
            else
            {
               startMousePosition = pos;
               mouseDownCount = 0;
            }
            mouseDownCount++;
         }
         else mouseDownCount = 0;
      }

      #region
      private void Grid_MouseMove_oldVersion(object sender, MouseEventArgs e)
      {
         if (mDown)
         {
            Point pos = Mouse.GetPosition(viewport);
            Point actualPos = new Point(pos.X - viewport.ActualWidth / 2, viewport.ActualHeight / 2 - pos.Y);
            double dx = actualPos.X - mLastPos.X, dy = actualPos.Y - mLastPos.Y;

            double mouseAngle = 0;
            if (dx != 0 && dy != 0)
            {
               mouseAngle = Math.Asin(Math.Abs(dy) / Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2)));
               if (dx < 0 && dy > 0) mouseAngle += Math.PI / 2;
               else if (dx < 0 && dy < 0) mouseAngle += Math.PI;
               else if (dx > 0 && dy < 0) mouseAngle += Math.PI * 1.5;
            }
            else if (dx == 0 && dy != 0) mouseAngle = Math.Sign(dy) > 0 ? Math.PI / 2 : Math.PI * 1.5;
            else if (dx != 0 && dy == 0) mouseAngle = Math.Sign(dx) > 0 ? 0 : Math.PI;

            double axisAngle = mouseAngle + Math.PI / 2;

            Vector3D axis = new Vector3D(Math.Cos(axisAngle) * 4, Math.Sin(axisAngle) * 4, 0);

            double rotation = 0.01 * Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));

            Transform3DGroup group = mGeometry.Transform as Transform3DGroup;
            QuaternionRotation3D r = new QuaternionRotation3D(new Quaternion(axis, rotation * 180 / Math.PI));
            group.Children.Add(new RotateTransform3D(r));

            mLastPos = actualPos;
         }
      }
      #endregion

      private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
      {
         this.mDown = e.LeftButton == MouseButtonState.Pressed;
         startMousePosition = Mouse.GetPosition(viewport);
         //processMouseNavigation(this.mDown);
      }

      private void Grid_MouseDown_old(object sender, MouseButtonEventArgs e)
      {
         if (e.LeftButton != MouseButtonState.Pressed) return;
         mDown = true;
         Point pos = Mouse.GetPosition(viewport);
         mLastPos = new Point(pos.X - viewport.ActualWidth / 2, viewport.ActualHeight / 2 - pos.Y);
      }

      private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
      {
         mDown = false;
      }

      private void btn_openData_Click(object sender, RoutedEventArgs e)
      {
         SystemSounds.Beep.Play();
         openDataFile();
      }

      private void openDataFile()
      {
         USGS_LakeLevelTxtFileReader =
            new USGS_LakeLevelTxtFileReader
               (@"C:\SourceModules\DataTicker3D\DataTicker3D\SampleData\FallsLakeAndHurricaneFran.txt");

         TimeTicker3D aTicker = new TimeTicker3D();
         aTicker.Brush = Brushes.Khaki;
         aTicker.rawData = USGS_LakeLevelTxtFileReader.getData();

         //ModelVisual3D modelVisual = new ModelVisual3D();
         //modelVisual.Content = aTicker.TickerGeometryModel3D;
         aTicker.TickerGeometryModel3D.BackMaterial = new DiffuseMaterial(Brushes.YellowGreen);
         aTicker.TickerGeometryModel3D.Material = new DiffuseMaterial(Brushes.Tomato);
         this.group.Children.Add(aTicker.TickerGeometryModel3D);
         //this.viewport.UpdateLayout();
      }

      private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
      {
         if (e.IsRepeat == true) return;
         if (keyIsDown.ContainsKey(e.Key) == true)
         {
            keyIsDown[e.Key] = true;
            e.Handled = true;
         }
         if (e.Key == Key.D)
            printDiagnostics = printDiagnostics == true ? false : true;

         processKeyPresses();
         //Debug.Print("Down " + e.Key.ToString());
      }

      private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
      {
         if (keyIsDown.ContainsKey(e.Key) == true)
            keyIsDown[e.Key] = false;

         processKeyPresses();
         //Debug.Print("Up " + e.Key.ToString());
      }

      private void processKeyPresses()
      {
         totalCameraMoveVector = new Vector3D(0, 0, 0);
         Double speedIncrement = 0.01;
         Double maxSpeed = 2.0;
         if (keyIsDown[Key.A] == true)
         {
            viewpointForwardSpeed += speedIncrement;
         }
         if (keyIsDown[Key.Z] == true)
         {
            viewpointForwardSpeed -= speedIncrement;
         }
         if (keyIsDown[Key.Space] == true)
         {
            viewpointForwardSpeed = 0;
         }
         if (keyIsDown[Key.R] == true)
         {
            this.camera.Position = new Point3D(
               cameraOriginalPosition.X, cameraOriginalPosition.Y, cameraOriginalPosition.Z);
            viewpointForwardSpeed = 0.0;
            this.viewpointVelocity = new Vector3D(0, 0, 0);
         }

         viewpointForwardSpeed = Math.Abs(viewpointForwardSpeed) > maxSpeed ?
            Math.Sign(viewpointForwardSpeed) * maxSpeed :
            viewpointForwardSpeed;

         Double zRotationSpeedIncrement = 0.2;
         Double maxZrotationSpeed = zRotationSpeedIncrement * 10;

         if (keyIsDown[Key.Right] == true)
         {
            viewpointRotationSpeedAboutWorldZ += zRotationSpeedIncrement;
         }
         if (keyIsDown[Key.Left] == true)
         {
            viewpointRotationSpeedAboutWorldZ -= zRotationSpeedIncrement;
         }
         zRotationSpeedIncrement = Math.Abs(zRotationSpeedIncrement) > maxZrotationSpeed ?
            Math.Sign(zRotationSpeedIncrement) * maxZrotationSpeed :
            zRotationSpeedIncrement;

         Double upAngle=0.0;   Double upAngleChange = 0;
         if(keyIsDown[Key.Down] == true) upAngleChange = viewpointUpAngleRotationSpeed;
         if(keyIsDown[Key.Up] == true) upAngleChange = -1 * viewpointUpAngleRotationSpeed;
         if (upAngleChange != 0.0)
         {
            upAngle = getUpAngle(this.camera.LookDirection);
            
            upAngle += upAngleChange;

            upAngle = Math.Abs(upAngle) > 86.0 ?
               Math.Sign(upAngle) * 86.0 :
               upAngle;
            myDebugPrint(upAngle.ToString());
            myDebugPrintObj(this.camera.LookDirection);
            this.camera.LookDirection =
               setVectorUpAngle(upAngle, this.camera.LookDirection);
            myDebugPrintObj(this.camera.LookDirection);
         }

         if (keyIsDown[Key.Left] || keyIsDown[Key.Right])
         {
            this.camera.LookDirection =
               rotateVector3DaboutWorldZ(viewpointRotationSpeedAboutWorldZ, this.camera.LookDirection);
         }
         else
         {
            zRotationSpeedIncrement = 0.0;
            viewpointRotationSpeedAboutWorldZ = 0.0;
         }

         var vectorToAdd = viewpointForwardSpeed * this.camera.LookDirection / 
            this.camera.LookDirection.Length;
         totalCameraMoveVector = totalCameraMoveVector + vectorToAdd;

         processSlewKeys();

         this.camera.Position += totalCameraMoveVector;

      }

      private Double maxSlewSpeed = 1.0;
      private Double xSlewSpeed = 0;
      private Double ySlewSpeed = 0;
      private Double zSlewSpeed = 0;
      private readonly Double slewDelta = 0.2;
      private void processSlewKeys()
      {
         if (keyIsDown[Key.NumPad4] == true || keyIsDown[Key.NumPad6] == true)
         {
            if (keyIsDown[Key.NumPad4] == true)
               xSlewSpeed -= slewDelta;
            else
               xSlewSpeed += slewDelta;
         }
         else
            xSlewSpeed = 0;

         if (keyIsDown[Key.NumPad8] == true || keyIsDown[Key.NumPad2] == true)
         {
            if (keyIsDown[Key.NumPad8] == true)
               ySlewSpeed -= slewDelta;
            else
               ySlewSpeed += slewDelta;
         }
         else
            ySlewSpeed = 0;

         if (keyIsDown[Key.Divide] == true || keyIsDown[Key.NumPad0] == true)
         {
            if (keyIsDown[Key.NumPad0] == true)
               zSlewSpeed -= slewDelta;
            else
               zSlewSpeed += slewDelta;
         }
         else
            zSlewSpeed = 0;

         totalCameraMoveVector += new Vector3D(xSlewSpeed, zSlewSpeed, ySlewSpeed);
      }

      private void myDebugPrint(String s)
      {
         if (true == printDiagnostics)
            Debug.Print(s);
      }

      private void myDebugPrintObj(Object obj)
      {
         myDebugPrint(obj.ToString());
      }

      private void testRotateVector()
      {
         return;
         var vec = new Vector3D(10, 15, 10);
         Debug.Print(getXYplaneAngleDegrees(vec).ToString());
         var rotatedVec = rotateVector3DaboutWorldZ(-45.0, vec);
         Debug.Print(getXYplaneAngleDegrees(rotatedVec).ToString());
      }

      private Vector3D rotateVector3DaboutWorldZ(Double rotation, Vector3D vec)
      {
         Double xyLength = getLengthProjToXYplane(vec);
         Double xyDirection = getXYplaneAngleDegrees(vec);
         xyDirection += rotation;
         Double xyDirRad = rad(xyDirection);
         return new Vector3D(
               xyLength * Math.Cos(xyDirRad),
               xyLength * Math.Sin(xyDirRad),
               vec.Z);
      }

      private Vector3D setVectorUpAngle(Double newUpAngle, Vector3D vec)
      {
         Double newZvalue = getLengthProjToXYplane(vec) * Math.Tan(rad(newUpAngle));
         return new Vector3D(
            vec.X,
            vec.Y,
            newZvalue);
      }

      private Double getLengthProjToXYplane(Vector3D vec)
      {
         return Math.Sqrt(vec.X * vec.X + vec.Y * vec.Y);
      }

      private Double getXYplaneAngleDegrees(Vector3D vec)
      {
         return deg(Math.Atan2(vec.Y, vec.X));
      }

      private Double getUpAngle(Vector3D vec)
      {
         return deg(Math.Atan2(vec.Z, getLengthProjToXYplane(vec)));
      }

      private Double deg(Double rad)
      {
         return 180 * rad / Math.PI;
      }

      private Double rad(Double deg)
      {
         return deg * Math.PI / 180;
      }

      private String vectorAsAngles(Vector3D vec)
      {
         if (vec.X == 0.0 && vec.Y == 0.0)
         {
            if (vec.Z == 0.0)
               return "point vector";
            else if (vec.Z > 0.0)
               return "Up";
            else
               return "Down";
         }
         return "Theta = " + deg(Math.Atan2(vec.Y, vec.X)) +
            "  Up Angle = " + deg(Math.Atan2(vec.Z, getLengthProjToXYplane(vec)));
      }
   }
}

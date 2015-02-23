using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

using GoblinXNA;
using GoblinXNA.Graphics;
using GoblinXNA.SceneGraph;
using Model = GoblinXNA.Graphics.Model;
using GoblinXNA.Graphics.Geometry;
using GoblinXNA.Device.Capture;
using GoblinXNA.Device.Vision;
using GoblinXNA.Device.Vision.Marker;
using GoblinXNA.Device.Util;
using GoblinXNA.Helpers;
using GoblinXNA.Shaders;
using GoblinXNA.Device.Generic;

using GoblinXNA.UI.UI2D;
using GoblinXNA.Physics;
using GoblinXNA.Physics.Newton1;
using GoblinXNA.UI;
using GoblinXNA.UI.UI3D;
using Nuclex.Fonts;

using Tutorial16___Multiple_Viewport;
using System.Text;



namespace Tutorial1___Getting_Started
{
    /// <summary>
    /// This tutorial demonstrates how the Camera works and the 
    /// Material and Light Property Variation of the 3D Objects
    /// </summary>
    public class Tutorial1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        Scene scene;

        // Space between view ports
        const int GAP = 10;

        // Light Source Components for Direction and Light Property
        LightSource lightSource; 
        LightNode lightNode; 
        Vector3 light_vector; 


        // The Material Applied to the HorseNode (Object Node) Geometry
        Material horseMaterial; 
        GeometryNode HorseNode;
        TransformNode HorsetransNode;
        Model ObjectModel;
        ModelLoader loader;

        // Camera - Position, Camera - LookAt Position, Camera - Up vector
        Vector3 cam_eye, init_cam_eye;    
        Vector3 cam_center, init_cam_center; 
        Vector3 cam_up, init_cam_up; 


        // Camera Node for View Port
        CameraNode leftCamNode; 
        CameraNode rightCamNode; 
        CameraNode bottomCamNode; 

        Camera rightCam;
        Camera leftCam;
       
        // Mouse Point Location
        int maxMousePoint = 0;
        int b_maxmousePoint = 0;
        int toggleObject = 0;

        // A sprite font to draw a 2D text string
        static string label;
        static string Info = " ";
        SpriteFont tutorialFont;
        SpriteFont UIFont;
        SpriteBatch spriteBatch;
        Texture2D image;
        bool b_help = false;
      
        // 2D UI panel
        G2DPanel frame;
        G2DLabel[] light_direction;
        G2DLabel[] light_direction_bounds;
        G2DLabel[] material;
        G2DLabel[] material_bounds;
        G2DLabel[] camera_direction_bounds;
        G2DLabel[] camera_direction;

       
        // View Port Setting
        RenderTarget2D leftViewRenderTarget;
        RenderTarget2D rightViewRenderTarget;
        RenderTarget2D bottomViewRenderTarget;
        Rectangle leftViewRect;
        Rectangle rightViewRect;
        Rectangle bottomViewRect;

        // Geometry and Transform Node for Frustrum
        GeometryNode vrCameraRepNode;
        GeometryNode planeNearNode1;
        GeometryNode planeNearNode2;
        GeometryNode planefarNode1;
        GeometryNode planefarNode2;
        TransformNode planeNearTransNode1;
        TransformNode planeNearTransNode2;
        TransformNode planefarTransNode1;
        TransformNode planefarTransNode2;

        // Transform Node for Virtual Camera 
        TransformNode vrCameraRepTransNode;
        TransformNode camOffset;
       
        Matrix viewMatrix;
        Point targetLocation;

        Vector3 xaxis;
        Vector3 yaxis;
        Vector3 zaxis;
        Vector3 vaxis;


        // Vector for X,Y and Z axis
        Vector3[] xarrow = new Vector3[4];
        Vector3[] yarrow = new Vector3[4];
        Vector3[] zarrow = new Vector3[4];
        Vector3[] varrow = new Vector3[4];

        float[] testaxis = new float[3];

        // Spribatch Render rectangles
        Rectangle imageRectangle;
        Rectangle[] modelObjectRect = new Rectangle[4];
        String[] modelObject = new String[4];
     
        public struct cell
        {
            int id, x, y;
            float min, max, cellvalue, step;
            string info, format;

            // constructor for using struct initialization
            public cell(int cell_id, int cell_x, int cell_y, float cell_min, float cell_max, float cell_value, float cell_step, string cell_info, string cell_format)
            {
                this.id = cell_id; this.x = cell_x; this.y = cell_y;
                this.min = cell_min; this.max = cell_max; this.cellvalue = cell_value;
                this.step = cell_step; this.info = cell_info; this.format = cell_format;
            }

            public int ID { get { return id; } set { id = value; } } // ID for each cell Structure
            public int X { get { return x; } set { x = value; } } 
            public int Y { get { return y; } set { y = value; } }
            public float MIN { get { return min; } set { min = value; } } // Minimum Value that the object can hold
            public float MAX { get { return max; } set { max = value; } } // Maximum Value the object can hold
            public float CELLVALUE { get { return cellvalue; } set { cellvalue = value; } } // Value of the object
            public float STEP { get { return step; } set { step = value; } } // Increase or decresing Step Value
            public string INFO { get { return info; } set { info = value; } } // Information about the Cell
            public string FORMAT { get { return format; } set { format = value; } } // The Format of the cell


        };


        // Structure for Light and Material
    
        cell[] cell_light = new cell[28] {

            // Cell Structure For Light Direction and Intensity Variation
            new cell(0, 180, 40, -5.0f, 5.0f, 1.0f, 0.1f,
                "Specifies X coordinate of light vector.", "%.2f"), 
            new cell(1, 240, 40, -5.0f, 5.0f, 1.0f, 0.1f,
                "Specifies Y coordinate of light vector.", "%.2f" ),
            new cell(2, 300, 40, -5.0f, 5.0f, -1.0f, 0.1f,
                "Specifies Z coordinate of light vector.", "%.2f"),
            new cell(3, 360, 40, 0.0f, 1.0f, 1.0f, 1.0f,
                "Specifies directional (0) or positional (1) light.", "%.2f" ),
        
            new cell(4, 180, 80, 0.0f, 1.0f, 0.1f, 0.01f,
                "Specifies Ambient Red Intensity of light .", "%.2f"), 
            new cell(5, 240, 80,  0.0f, 1.0f, 0.5f, 0.01f,
                "Specifies Ambient Green Intensity of light.", "%.2f" ),
            new cell(6, 300, 80, 0.0f, 1.0f, 0.0f, 0.01f,
                "Specifies Ambient Blue Intensity of light.", "%.2f"),
            new cell(7, 360, 80, 0.0f, 1.0f, 1.0f, 0.1f,
                "Specifies alpha Intensity light.", "%.2f" ),


            new cell(8, 180, 120, 0.0f, 1.0f, 0.1f, 0.01f,
                "Specifies Diffuse Red Intensity of light .", "%.2f"), 
            new cell(9, 240, 120, 0.0f, 1.0f, 0.1f, 0.01f,
                "Specifies Diffuse Green Intensity of light.", "%.2f" ),
            new cell(10, 300, 120, 0.0f, 1.0f, 0.1f, 0.01f,
                "Specifies Diffuse Blue Intensity of light.", "%.2f"),
            new cell(11, 360, 120, 0.0f, 1.0f, 1.0f, 0.1f,
                "Specifies alpha Intensity light.", "%.2f" ),   

            new cell(12, 180, 160, 0.0f, 1.0f, 0.5f, 0.01f,
                "Specifies Specular Red Intensity of light.", "%.2f"), 
            new cell(13, 240, 160, 0.0f, 1.0f, 0.5f, 0.01f,
                "Specifies Specular Green Intensity of light.", "%.2f" ),
            new cell(14, 300, 160, 0.0f, 1.0f, 0.0f, 0.01f,
                "Specifies Specular Blue Intensity of light.", "%.2f"),
            new cell(15, 360, 160, 0.0f, 1.0f, 0.0f, 0.1f,
                "Specifies alpha Intensity light.", "%.2f" ),
          
                // Cell Structure For Material Reflectance Variation
            new cell(16, 180, 200, 0.0f, 1.0f, 0.1f, 0.01f,
                "Specifies Ambient Red Reflectance of Material .", "%.2f"), 
            new cell(17, 240, 200, 0.0f, 1.0f, 0.5f, 0.01f,
                "Specifies Ambient Green Reflectance of Material .", "%.2f" ),
            new cell(18, 300, 200, 0.0f, 1.0f, 0.0f, 0.01f,
                "Specifies Ambient Blue Reflectance of Material .", "%.2f"),
            new cell(19, 360, 200, 0.0f, 1.0f, 1.0f, 0.1f,
                "Specifies alpha Intensity light.", "%.2f" ),


            new cell(20, 180, 240, 0.0f, 1.0f, 0.1f, 0.01f,
                "Specifies Diffuse Red Reflectance of Material  .", "%.2f"), 
            new cell(21, 240, 240, 0.0f, 1.0f, 0.1f, 0.01f,
                "Specifies Diffuse Green Reflectance of Material .", "%.2f" ),
            new cell(22, 300, 240, 0.0f, 1.0f, 0.1f, 0.01f,
                "Specifies Diffuse Blue Reflectance of Material .", "%.2f"),
            new cell(23, 360, 240, 0.0f, 1.0f, 1.0f, 0.1f,
                "Specifies alpha Intensity light.", "%.2f" ),   

            new cell(24, 180, 280, 0.0f, 1.0f, 0.5f, 0.01f,
                "Specifies Specular Red Reflectance of Material .", "%.2f"), 
            new cell(25, 240, 280, 0.0f, 1.0f, 0.5f, 0.01f,
                "Specifies Specular Green Reflectance of Material .", "%.2f" ),
            new cell(26, 300, 280, 0.0f, 1.0f, 0.0f, 0.01f,
                "Specifies Specular Blue Reflectance of Material .", "%.2f"),
            new cell(27, 360, 280, 0.0f, 1.0f, 1.0f, 0.1f,
                "Specifies alpha Intensity light.", "%.2f" ),
          
           };


        // Cell Structure For Camera LookAt
        cell[] cell_lookAt = new cell[9] {
            new cell ( 28, 180, 200, -5.0f, 5.0f, 0.0f, 0.1f,
        "Specifies the X position of the eye point.", "%.2f" ),
            new cell ( 29, 240, 200, -5.0f, 5.0f, 0.0f, 0.1f,
        "Specifies the Y position of the eye point.", "%.2f" ),
            new cell ( 30, 300, 200, -5.0f, 5.0f, 2.0f, 0.1f,
        "Specifies the Z position of the eye point.", "%.2f" ),
            new cell ( 31, 180, 240, -5.0f, 5.0f, 0.0f, 0.1f,
        "Specifies the X position of the reference point.", "%.2f" ),
            new cell ( 32, 240, 240, -5.0f, 5.0f, 0.0f, 0.1f,
        "Specifies the Y position of the reference point.", "%.2f" ),
            new cell ( 33, 300, 240, -5.0f, 5.0f, -5.0f, 0.1f,
        "Specifies the Z position of the reference point.", "%.2f" ),
            new cell ( 34, 180, 280, -1.0f, 1.0f, 0.0f, 0.1f,
        "Specifies the X direction of the up vector.", "%.2f" ),
            new cell ( 35, 240, 280, -1.0f, 1.0f, 1.0f, 0.1f,
        "Specifies the Y direction of the up vector.", "%.2f" ),
            new cell ( 36, 300, 280, -1.0f, 1.0f, 0.0f, 0.1f,
        "Specifies the Z direction of the up vector.", "%.2f" )};



        public Tutorial1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 600;
            Content.RootDirectory = "Content";

             this.IsMouseVisible = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        /// 

        protected override void Initialize()
        {
            base.Initialize();

#if WINDOWS
            // Display the mouse cursor
            this.IsMouseVisible = true;
#endif
           // spriteBatch = 
            // Initialize the GoblinXNA framework
            State.InitGoblin(graphics, Content, "");
           
            // Initialize the scene graph
            scene = new Scene();
            label = "";

            light_vector = new Vector3((float)cell_light[0].CELLVALUE, (float)cell_light[1].CELLVALUE, (float)cell_light[2].CELLVALUE);

            
            // Set up the cameraEye, lookAt and Up Vector
            cam_eye = new Vector3((float)cell_lookAt[0].CELLVALUE, (float)cell_lookAt[1].CELLVALUE, (float)cell_lookAt[2].CELLVALUE);
            cam_center = new Vector3((float)cell_lookAt[3].CELLVALUE, (float)cell_lookAt[4].CELLVALUE, (float)cell_lookAt[5].CELLVALUE);
            cam_up = new Vector3((float)cell_lookAt[6].CELLVALUE, (float)cell_lookAt[7].CELLVALUE, (float)cell_lookAt[8].CELLVALUE);

            // Initial CameraPosition and LookAt Direction
            init_cam_eye = cam_eye;
            init_cam_center = cam_center;
            init_cam_up = cam_up;

            // Set up the 3 View Ports
            SetupViewport();

            // Set up the lights used in the scene with the values from the cell structure
            CreateLights();

            // Set up the camera which defines the eye location and viewing frustum
            CreateCameras();

            // Create Plane for Bounding Frustum
            CreateBoundingFrustum();

            // Create 3D objects
            CreateObjects();

            // Set up a Virtual camera 
            CreateCameraRepresentation();

            // Set up the UI Screen for third View Port
            Create2DUIScreen();

            Info = " H - Help ";
            spriteBatch = new SpriteBatch(this.GraphicsDevice);
        
            KeyboardInput.Instance.KeyPressEvent += new HandleKeyPress(Instance_KeyPress);
            MouseInput.Instance.MouseClickEvent  += new HandleMouseClick(Instance_MouseClick);
            State.ShowFPS = false;
        }

        // Initial Creation
        // Creation of Light, Material, Camera and UI screen
        private void SetupViewport()
        {
            // Create Three View Ports
            PresentationParameters pp = GraphicsDevice.PresentationParameters;

            // Left View Port
            // Shows the Virtual representation of Right View Camera and the Object
            leftViewRenderTarget = new RenderTarget2D(GraphicsDevice, graphics.PreferredBackBufferWidth / 2 - GAP,
                graphics.PreferredBackBufferHeight * 2 / 3 - 2 * GAP, false, SurfaceFormat.Color, pp.DepthStencilFormat);

            leftViewRect = new Rectangle(0 + GAP, 0 + GAP, graphics.PreferredBackBufferWidth / 2 - GAP, graphics.PreferredBackBufferHeight * 2 / 3 - 2 * GAP);

            // Right View Port
            // Shows the world View Projection of the Right View Camera
            rightViewRenderTarget = new RenderTarget2D(GraphicsDevice, graphics.PreferredBackBufferWidth / 2 - GAP,
                graphics.PreferredBackBufferHeight * 2 / 3 - 2 * GAP, false, SurfaceFormat.Color, pp.DepthStencilFormat);

            rightViewRect = new Rectangle(graphics.PreferredBackBufferWidth / 2 + GAP, 0 + GAP, graphics.PreferredBackBufferWidth / 2 - 2 * GAP,
               graphics.PreferredBackBufferHeight * 2 / 3 - 2 * GAP);

            // Bottom View Port
            // For Drawing the 2D UI Screen used for manipulating the Light, Material and Camera Properties
            bottomViewRenderTarget = new RenderTarget2D(GraphicsDevice, graphics.PreferredBackBufferWidth - 2 * GAP,
                graphics.PreferredBackBufferHeight / 3 - 2 * GAP, false, SurfaceFormat.Color, pp.DepthStencilFormat);
 
            bottomViewRect = new Rectangle(0 + GAP, graphics.PreferredBackBufferHeight * 2 / 3, graphics.PreferredBackBufferWidth - 2 * GAP,
                graphics.PreferredBackBufferHeight / 3 - GAP);

        }

        private void CreateLights()
        {
            // Create a directional light source
            lightSource = new LightSource();
            lightSource.Direction = light_vector;
            lightSource.Diffuse = Color.White.ToVector4();
            lightSource.Specular = new Vector4(0.6f, 0.6f, 0.6f, 1);
           
            // Create a light node to hold the light source
            lightNode = new LightNode();
            lightNode.AmbientLightColor = new Vector4(0.5f, 0.5f, 0.5f, 1);          
            lightNode.LightSource = lightSource;
            scene.RootNode.AddChild(lightNode);
        }

        private void CreateCameras()
        {
            // Create Left Camera for Left View Port
            leftCam = new Camera();
            leftCam.FieldOfViewY = MathHelper.ToRadians(60);
            leftCam.View = Matrix.CreateLookAt(new Vector3(10, 0, 5), new Vector3(0, 0, -4), new Vector3(0, 1, 0)); // Camera is in fixed location
            leftCam.AspectRatio = leftViewRenderTarget.Width / (float)leftViewRenderTarget.Height;
            leftCam.ZNearPlane = 1;
            leftCam.ZFarPlane = 2000;
            leftCamNode = new CameraNode(leftCam);
            scene.RootNode.AddChild(leftCamNode);

            // Create Right Camera for Right View Port
            rightCam = new Camera();
            rightCam.FieldOfViewY = MathHelper.ToRadians(60);
            rightCam.AspectRatio = rightViewRenderTarget.Width / (float)rightViewRenderTarget.Height;
            rightCam.ZNearPlane = 1f;
            rightCam.ZFarPlane = 1000;
            rightCam.Translation = cam_eye;
            rightCam.View = Matrix.CreateLookAt(cam_eye, cam_center, cam_up);
            rightCam.Projection = Matrix.CreatePerspectiveFieldOfView(rightCam.FieldOfViewY, rightCam.AspectRatio, rightCam.ZNearPlane, rightCam.ZFarPlane);
            rightCamNode = new CameraNode(rightCam);
            scene.RootNode.AddChild(rightCamNode);

            // Create Camera for Bottom view Port
            Camera bottomCam = new Camera();
            bottomCam.Translation = new Vector3(0, -2, 0);
            bottomCam.FieldOfViewY = MathHelper.ToRadians(60);
            bottomCam.AspectRatio = bottomViewRenderTarget.Width / (float)bottomViewRenderTarget.Height;
            bottomCam.ZNearPlane = 1;
            bottomCam.ZFarPlane = 2000;

            bottomCamNode = new CameraNode(bottomCam);
            scene.RootNode.AddChild(bottomCamNode);

            scene.CameraNode = leftCamNode;
        }
     
        private void CreateObjects()
        {

            image = Content.Load<Texture2D>("BaseImage");
            // Loads a textured model of a Horse
            loader = new ModelLoader();
            ObjectModel = (Model)loader.Load("", "3D_Horse_fbx");            
            
            // Create a geometry node of a loaded Horse model
            HorseNode = new GeometryNode("3D_Horse_fbx");
            HorseNode.Model = ObjectModel;
         
            // Create a Material for Horse Geometry
            horseMaterial = new Material();
            horseMaterial.Ambient = Color.Wheat.ToVector4();
            horseMaterial.Diffuse = Color.Wheat.ToVector4();

            HorseNode.Material = horseMaterial;

            HorsetransNode = new TransformNode()
            {
                Translation = cam_center, // Place the Horse Geometry in the Location calculated from the Value of Cell_Camera
                Scale = new Vector3(0.002f, 0.002f, 0.002f), // Scale it as size is huge

            };
           
            scene.RootNode.AddChild(HorsetransNode);
            HorsetransNode.AddChild(HorseNode);

            // Load Image names

            modelObject[0] = " GingerbreadMan ";
            modelObject[1] = " kangaroo ";
            modelObject[2] = " Wedge ";
            modelObject[3] = " Horse ";

        }

        private void CreateBoundingFrustum()
        {
            // Get the Corners of the Bounding Frustum of the Camera Node of the Right View Port
            Vector3[] corners = new Vector3[8];
            corners = rightCamNode.BoundingFrustum.GetCorners();

            // Get the View Matrix of the Camera
            viewMatrix = CreateViewMatrix(cam_eye, cam_center, cam_up);
            createAxis(viewMatrix);


            float xdim = Math.Abs( Vector3.Distance(corners[0], corners[1]));
            float ydim = Math.Abs(Vector3.Distance(corners[0], corners[3]));
            float xdimFar = Math.Abs(Vector3.Distance(corners[4], corners[5]));
            float ydimFar = Math.Abs(Vector3.Distance(corners[4], corners[7]));

            // Create four Planes for Bounding Frustum Representation
            // Two Planes for Near Frustum
            TexturedPlane planeNear1 = new TexturedPlane(xdim, ydim);
            TexturedPlane planeNear2 = new TexturedPlane(xdim, ydim);
            // Two Planes for Far Frustum
            TexturedPlane planeFar1 = new TexturedPlane(xdimFar, ydimFar);
            TexturedPlane planeFar2 = new TexturedPlane(xdimFar, ydimFar);

            planeNearNode1 = new GeometryNode("PlaneNear1");
            planeNearNode2 = new GeometryNode("PlaneNear2");
            planefarNode1 = new GeometryNode("PlaneFar1");
            planefarNode2 = new GeometryNode("Planefar2");

            // Textured Plane Model applied to the Plane Geometry
            planeNearNode1.Model = planeNear1;
            planeNearNode2.Model = planeNear2;
            planefarNode1.Model = planeFar1;
            planefarNode2.Model = planeFar2;

            planeNearTransNode1 = new TransformNode();
            planeNearTransNode2 = new TransformNode();
            planefarTransNode1 = new TransformNode();
            planefarTransNode2 = new TransformNode();

            Material planeMaterial = new Material();
            planeMaterial.Diffuse = new Vector4(0.5f, 0.5f, 0.5f, 0.2f);

            planeNearNode1.Material = planeMaterial;
            planeNearNode2.Material = planeMaterial;
            planefarNode1.Material = planeMaterial;
            planefarNode2.Material = planeMaterial;


            // Orient the Plane to match the Bounding Frustum
            planeNearTransNode1.Translation = (corners[0] + corners[1] + corners[2] + corners[3]) / 4;
            planeNearTransNode1.Rotation = Quaternion.CreateFromRotationMatrix(viewMatrix);
            planeNearTransNode1.Rotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathHelper.PiOver2);

            planeNearTransNode2.Translation = (corners[0] + corners[1] + corners[2] + corners[3]) / 4;
            planeNearTransNode2.Rotation = Quaternion.CreateFromRotationMatrix(viewMatrix);
            planeNearTransNode2.Rotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitX, -MathHelper.PiOver2);


            planefarTransNode1.Translation = (corners[4] + corners[5] + corners[6] + corners[7]) / 4;
            planefarTransNode1.Rotation = Quaternion.CreateFromRotationMatrix(viewMatrix);
            planefarTransNode1.Rotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathHelper.PiOver2);

            planefarTransNode2.Translation = (corners[4] + corners[5] + corners[6] + corners[7]) / 4;
            planefarTransNode2.Rotation = Quaternion.CreateFromRotationMatrix(viewMatrix);
            planefarTransNode2.Rotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitX, -MathHelper.PiOver2);


            planeNearTransNode1.AddChild(planeNearNode1);
            planeNearTransNode2.AddChild(planeNearNode2);
            planefarTransNode1.AddChild(planefarNode1);
            planefarTransNode2.AddChild(planefarNode2);

            scene.RootNode.AddChild(planeNearTransNode1);
            scene.RootNode.AddChild(planeNearTransNode2);
            scene.RootNode.AddChild(planefarTransNode1);
            scene.RootNode.AddChild(planefarTransNode2);

        }

        private void CreateCameraRepresentation()
        {
            // Virtual camera orientation with respect to camera 
            vrCameraRepNode = new GeometryNode("VR Camera")
            {
                Model = new Pyramid(0.5f, 0.5f, 0.5f),
                Material =
                {
                    Diffuse = Color.Orange.ToVector4(),
                    Specular = Color.White.ToVector4(),
                    SpecularPower = 20
                }

            };

            vrCameraRepTransNode = new TransformNode();
            camOffset = new TransformNode()
            {
                // Orient the Virtual Camera transformation to match the Right View Camera Transformation
                Rotation = Quaternion.CreateFromRotationMatrix(CreateViewMatrix(cam_eye, cam_center, cam_up)),
                Translation = cam_eye,
            };

            scene.RootNode.AddChild(vrCameraRepTransNode);
            vrCameraRepTransNode.AddChild(camOffset);
            camOffset.AddChild(vrCameraRepNode);

        }

        private void Create2DUIScreen()
        {
            int Dbv = 20;
           
            // Create the main panel which holds all other GUI components
            frame = new G2DPanel();
            frame.Bounds = new Rectangle(0, 0, graphics.PreferredBackBufferWidth - 2 * GAP,
              graphics.PreferredBackBufferHeight / 3 - GAP);
            frame.Border = GoblinEnums.BorderFactory.EtchedBorder;
            frame.BackgroundColor = Color.White;
            frame.Transparency = 0.7f;  // Ranges from 0 (fully transparent) to 1 (fully opaque)


            int F_boundX = 370 + frame.Bounds.X;
            tutorialFont = Content.Load<SpriteFont>("Arial-bold");
            UIFont = Content.Load<SpriteFont>("UIFont");
           
            // Label for Light Components
            G2DLabel cell_light_label_start = new G2DLabel("Light Source_pos[] =    {       ");
            G2DLabel cell_light_label_end = new G2DLabel(" } ");
            G2DLabel cell_light_ambient_label_start = new G2DLabel("Light Source_Ka[] =     {       ");
            G2DLabel cell_light_ambient_label_end = new G2DLabel(" } ");
            G2DLabel cell_light_diffuse_label_start = new G2DLabel("Light Source_Kd[] =     {       ");
            G2DLabel cell_light_diffuse_label_end = new G2DLabel(" } ");
            G2DLabel cell_light_specular_label_start = new G2DLabel("Light Source_Ks[] =      {     ");
            G2DLabel cell_light_specular_label_end = new G2DLabel(" } ");

            // Label for Material Components
            G2DLabel cell_material_ambient_label_start = new G2DLabel("Horse Material_Ka[] =    {   ");
            G2DLabel cell_material_ambient_label_end = new G2DLabel(" } ");
            G2DLabel cell_material_diffuse_label_start = new G2DLabel("Horse Material_Kd[] =    {   ");
            G2DLabel cell_material_diffuse_label_end = new G2DLabel(" } ");
            G2DLabel cell_material_specular_label_start = new G2DLabel("Horse Material_ks[] =     {  ");
            G2DLabel cell_material_specular_label_end = new G2DLabel(" } ");

            // Label for Camera LookAt Component
            G2DLabel cell_camera_label_start = new G2DLabel("Camera LookAt Direction { ");
            G2DLabel cell_camera_label_end = new G2DLabel(" } ");

            // Set The Label Bounds for Light 
            cell_light_label_start.Bounds = new Rectangle(F_boundX, frame.Bounds.Y + 10, 150, 10);
            cell_light_ambient_label_start.Bounds = new Rectangle(F_boundX, frame.Bounds.Y + 10 + Dbv * 1, 150, 10);
            cell_light_diffuse_label_start.Bounds = new Rectangle(F_boundX, frame.Bounds.Y + 10 + Dbv * 2, 150, 10);
            cell_light_specular_label_start.Bounds = new Rectangle(F_boundX, frame.Bounds.Y + 10 + Dbv * 3, 150, 10);

            // set the Label Bounds for Material
            cell_material_ambient_label_start.Bounds = new Rectangle(F_boundX, frame.Bounds.Y + 10 + Dbv * 5, 150, 10);
            cell_material_diffuse_label_start.Bounds = new Rectangle(F_boundX, frame.Bounds.Y + 10 + Dbv * 6, 150, 10);
            cell_material_specular_label_start.Bounds = new Rectangle(F_boundX, frame.Bounds.Y + 10 + Dbv * 7, 150, 10);

            // Set the Label Bounds fot Camera
            cell_camera_label_start.Bounds = new Rectangle(frame.Bounds.X + 10, frame.Bounds.Y + 10, 150, 10);

            // Loads the font for the tutorial demo rendering the value for light , material and camera

            // Set the Font for the Cell Light Value to be written from the CELLVALUE of each cell
            cell_light_label_start.TextFont = tutorialFont;
            cell_light_diffuse_label_start.TextFont = tutorialFont;
            cell_light_specular_label_start.TextFont = tutorialFont;
            cell_light_ambient_label_start.TextFont = tutorialFont;

            // Set the Font for the Cell material Value to be written from the CELLVALUE of each cell
            cell_material_ambient_label_start.TextFont = tutorialFont;
            cell_material_diffuse_label_start.TextFont = tutorialFont;
            cell_material_specular_label_start.TextFont = tutorialFont;

            // Set the Font for the Cell camera Value to be written from the CELLVALUE of each cell
            cell_camera_label_start.TextFont = tutorialFont;


            // Add the Light Label to 2DUI frame
            frame.AddChild(cell_light_label_start);
            frame.AddChild(cell_light_diffuse_label_start);
            frame.AddChild(cell_light_ambient_label_start);
            frame.AddChild(cell_light_specular_label_start);

            // Add the material Label to 2DUI frame
            frame.AddChild(cell_material_diffuse_label_start);
            frame.AddChild(cell_material_ambient_label_start);
            frame.AddChild(cell_material_specular_label_start);

            // Add the camera Label to the 2DUI frame
            frame.AddChild(cell_camera_label_start);


           
            light_direction = new G2DLabel[28];
            light_direction_bounds = new G2DLabel[28];

            material = new G2DLabel[12];
            material_bounds = new G2DLabel[12];

            camera_direction_bounds = new G2DLabel[9];
            camera_direction = new G2DLabel[9];

            // Use the cell value for Light and Material  to update  in the 2D UI Screen
            int k = 0;
            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    light_direction[k] = new G2DLabel(cell_light[k].CELLVALUE.ToString());
                    light_direction_bounds[k] = new G2DLabel();
                   
                    
                    if (i > 3)
                    {
                        light_direction[k].Bounds = new Rectangle(cell_light_label_start.Bounds.Width + cell_light_label_start.Bounds.X + 10 + j * 40,
                                                           cell_light_label_start.Bounds.Y + (i * Dbv) + Dbv, 40, 10);
                        light_direction_bounds[k].Bounds = new Rectangle(cell_light_label_start.Bounds.Width + cell_light_label_start.Bounds.X + 10 + j * 40,
                                                                  (graphics.PreferredBackBufferHeight * 2 / 3) + (((i - 1) * Dbv) + Dbv), 40, 80);

                    }
                   
                    else
                    {
                        light_direction[k].Bounds = new Rectangle(cell_light_label_start.Bounds.Width + cell_light_label_start.Bounds.X + 10 + j * 40,
                                                                cell_light_label_start.Bounds.Y + i * Dbv, 40, 10);
                        light_direction_bounds[k].Bounds = new Rectangle(cell_light_label_start.Bounds.Width + cell_light_label_start.Bounds.X + 10 + j * 40,
                                                                  (graphics.PreferredBackBufferHeight * 2 / 3) + ((i - 1) * Dbv), 40, 80);
                    }
                    Console.WriteLine(light_direction[k].Bounds.ToString());
                    light_direction[k].TextFont = tutorialFont;
                    light_direction[k].TextColor = Color.Green;
                    light_direction_bounds[k].MouseDraggedEvent += new MouseDragged(MouseDragHandlerLight);
                    frame.AddChild(light_direction[k]);
                    frame.AddChild(light_direction_bounds[k]);
                    k++;
                }
            }
            
            // Cell Light End Bounds
            cell_light_label_end.Bounds = new Rectangle(light_direction[3].Bounds.Right - 10, light_direction[3].Bounds.Y, 20, 10);
            cell_light_ambient_label_end.Bounds = new Rectangle(light_direction[7].Bounds.Right - 10, light_direction[7].Bounds.Y, 20, 10);
            cell_light_diffuse_label_end.Bounds = new Rectangle(light_direction[11].Bounds.Right - 10, light_direction[11].Bounds.Y, 20, 10);
            cell_light_specular_label_end.Bounds = new Rectangle(light_direction[15].Bounds.Right - 10, light_direction[15].Bounds.Y, 20, 10);

            // Cell Light Label End font
            cell_light_label_end.TextFont = tutorialFont;
            cell_light_ambient_label_end.TextFont = tutorialFont;
            cell_light_diffuse_label_end.TextFont = tutorialFont;
            cell_light_specular_label_end.TextFont = tutorialFont;

            frame.AddChild(cell_light_label_end);
            frame.AddChild(cell_light_diffuse_label_end);
            frame.AddChild(cell_light_specular_label_end);
            frame.AddChild(cell_light_ambient_label_end);

            cell_material_ambient_label_end.Bounds = new Rectangle(light_direction[19].Bounds.Right -10, light_direction[19].Bounds.Y, 20, 10);
            cell_material_diffuse_label_end.Bounds = new Rectangle(light_direction[23].Bounds.Right-10, light_direction[23].Bounds.Y, 20, 10);
            cell_material_specular_label_end.Bounds = new Rectangle(light_direction[27].Bounds.Right-10, light_direction[27].Bounds.Y, 20, 10);

            cell_material_ambient_label_end.TextFont = tutorialFont;
            cell_material_diffuse_label_end.TextFont = tutorialFont;
            cell_material_specular_label_end.TextFont = tutorialFont;

            frame.AddChild(cell_material_diffuse_label_end);
            frame.AddChild(cell_material_specular_label_end);
            frame.AddChild(cell_material_ambient_label_end);


           // Use the cell value for camera to update  in the 2D UI Screen
            k = 0;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    camera_direction[k] = new G2DLabel(cell_lookAt[k].CELLVALUE.ToString());
                    camera_direction_bounds[k] = new G2DLabel();

                    camera_direction[k].Bounds = new Rectangle(cell_camera_label_start.Bounds.Width + cell_camera_label_start.Bounds.X + 30 + j * 40,
                                                              cell_camera_label_start.Bounds.Y + i * 25, 40, 10);
                    camera_direction_bounds[k].Bounds = new Rectangle(cell_camera_label_start.Bounds.Width + cell_camera_label_start.Bounds.X + 30 + j * 40,
                                                       (graphics.PreferredBackBufferHeight * 2 / 3) + ((i - 1) * 25), 40, 80);

                    camera_direction[k].TextFont = tutorialFont;
                    camera_direction[k].TextColor = Color.Green;
                    camera_direction_bounds[k].MouseDraggedEvent += new MouseDragged(MouseDragHandlerCamera);
                    frame.AddChild(camera_direction[k]);
                    frame.AddChild(camera_direction_bounds[k]);
                    k++;
                }
            }

            cell_camera_label_end.Bounds = new Rectangle(camera_direction[8].Bounds.Right - 20, camera_direction[8].Bounds.Y , 20, 10);
            cell_camera_label_end.TextFont = tutorialFont;
            frame.AddChild(cell_camera_label_end);

            light_direction[0].Name = "LightX";
            light_direction[1].Name = "LightY";
            light_direction[2].Name = "LightZ";

            scene.UIRenderer.Add2DComponent(frame);

        }


     /*   private string WrapText(string text)
        {
            string[] words = text.Split(' ');
            StringBuilder sb = new StringBuilder();
            float linewidth = 0f;
            float maxLine = 250f; //a bit smaller than the box so you can have some padding...etc
            float spaceWidth = spriteFont.MeasureString(" ").X;

            foreach (string word in words)
            {
                Vector2 size = spriteFont.MeasureString(word);
                if (linewidth + size.X < 250)
                {
                    sb.Append(word + " ");
                    linewidth += size.X + spaceWidth;
                }
                else
                {
                    sb.Append("\n" + word + " ");
                    linewidth = size.X + spaceWidth;
                }
            }
            return sb.ToString();
        }*/

        // Update Functions
        // For Updating Light, Material and Camera

        private void UpdateLights(Vector4 light_vector_update, int i)
        {
            // Update the Direction of the Light
            if (i == 0)
                lightSource.Direction = new Vector3(light_vector_update.X, light_vector_update.Y, light_vector_update.Z);

            // Update the Ambient Color Intensity of the Light
            else if (i == 1)
                lightNode.AmbientLightColor = new Vector4(light_vector_update.X, light_vector_update.Y, light_vector_update.Z, light_vector_update.W);

            // Update the Diffuse Color Intensity of the Light
            else if (i == 2)
                lightSource.Diffuse = new Vector4(light_vector_update.X, light_vector_update.Y, light_vector_update.Z, light_vector_update.W);

            // Update the Specualr Color Intensity of the Light
            else if (i == 3)
                lightSource.Specular = new Vector4(light_vector_update.X, light_vector_update.Y, light_vector_update.Z, light_vector_update.W);


        }

        private void UpdateMaterial(Vector4 material_vector_update, int i)
        {
            // Update the Ambient Material Reflectance of Material
            if (i == 4)
                horseMaterial.Ambient = new Vector4(material_vector_update.X, material_vector_update.Y, material_vector_update.Z, material_vector_update.W);

            // Update the Diffuce Material Reflectance of Material
            else if (i == 5)
                horseMaterial.Diffuse = new Vector4(material_vector_update.X, material_vector_update.Y, material_vector_update.Z, material_vector_update.W);

            // Update the Specular Material Reflectance of Material
            else if (i == 6)
                horseMaterial.Specular = new Vector4(material_vector_update.X, material_vector_update.Y, material_vector_update.Z, material_vector_update.W);

           }

        private void UpdateCamera(Vector4 camera_Vector, int value)
        {
            Vector3 Camera_Vector = new Vector3(camera_Vector.X, camera_Vector.Y, camera_Vector.Z);
            viewMatrix = new Matrix();
            switch (value)
            {
                    // Update the Camera Eye Position
                case 0:

                    cam_eye = Camera_Vector;
                    rightCam.View = Matrix.CreateLookAt(Camera_Vector, cam_center, cam_up);
                    viewMatrix = CreateViewMatrix(cam_eye, cam_center, cam_up);
                    camOffset.Rotation = Quaternion.CreateFromRotationMatrix(viewMatrix);
                    camOffset.Translation = cam_eye;
                    createAxis(viewMatrix);

                    break;

                    // Update the Camera Lookat Position
                case 1:
                    cam_center = Camera_Vector;
                    rightCam.View = Matrix.CreateLookAt(cam_eye, Camera_Vector, cam_up);
                    viewMatrix = CreateViewMatrix(cam_eye, cam_center, cam_up);
                    camOffset.Rotation = Quaternion.CreateFromRotationMatrix(viewMatrix);
                    camOffset.Translation = cam_eye;
                    createAxis(viewMatrix);

                    break;

                    // Update the Camera Up Vector
                case 2:
                    cam_up = Camera_Vector;
                    rightCam.View = Matrix.CreateLookAt(cam_eye, cam_center, Camera_Vector);
                    viewMatrix = CreateViewMatrix(cam_eye, cam_center, cam_up);
                    camOffset.Rotation = Quaternion.CreateFromRotationMatrix(viewMatrix);
                    camOffset.Translation = cam_eye;
                    createAxis(viewMatrix);

                    break;
            }

        }

        // Key Board Event to reset and Exit
        private void Instance_KeyPress(Keys key, KeyModifier target)
        {
            if (Keys.R == key)
            {
                ResetComponents(); 
            }
            if (Keys.Escape == key)
            {
                Exit();
            }
            if (Keys.T == key)
            {
                KeyBoardToggleObjects();
            }
            if (Keys.H == key)
            {

                b_help = !b_help;
            }
        }

        private void Instance_MouseClick(int button, Point target )
        {
            targetLocation = target;

            if (button == MouseInput.RightButton)
            {
                b_help = true;
            }
            if (button == MouseInput.LeftButton)
            {   
                if(b_help)
                {
                    if (!(target.X > imageRectangle.Left && target.X < imageRectangle.Right)
                        && (target.Y > imageRectangle.Top && target.Y < imageRectangle.Bottom))
                        b_help = false;
                    else
                    { ToggleObjects((target.Y - imageRectangle.Y) / 25); b_help = false; }
                }
                    
            }

        }

        // Reset the Settings 
        private void ResetComponents()
        {
            // Light Reset              
            lightSource.Direction = light_vector;
            lightSource.Diffuse = Color.White.ToVector4();
            lightSource.Specular = new Vector4(0.6f, 0.6f, 0.6f, 1);
            lightSource.Type = LightType.Directional;
            lightNode.AmbientLightColor = new Vector4(0.5f, 0.5f, 0.5f, 1);
            lightNode.LightSource = lightSource;

            // Camera Reset
            cam_eye = init_cam_eye;
            cam_center = init_cam_center;
            cam_up = init_cam_up;
            rightCam.View = Matrix.CreateLookAt(cam_eye, cam_center, cam_up);
            viewMatrix = CreateViewMatrix(cam_eye, cam_center, cam_up);
            camOffset.Rotation = Quaternion.CreateFromRotationMatrix(viewMatrix);
            camOffset.Translation = cam_eye;


            // Material Reset
            horseMaterial.Ambient = Color.Wheat.ToVector4();
            horseMaterial.Diffuse = Color.Wheat.ToVector4();
            HorsetransNode.Translation = cam_center;

            // Virtual Camera Reset       
            camOffset.Rotation = Quaternion.CreateFromRotationMatrix(CreateViewMatrix(cam_eye, cam_center, cam_up));
            camOffset.Translation = cam_eye;

            // Axis Reset
            createAxis(viewMatrix);         
        }

        private void ToggleObjects(int whichObject)
        {
            switch (whichObject)
            {
                case 0:
                    ObjectModel = (Model)loader.Load("", "GingerbreadManFBX");
                    HorsetransNode.Scale = new Vector3(0.009f, 0.009f, 0.009f);
                    break;
                case 1:
                    ObjectModel = (Model)loader.Load("", "kanga");
                    HorsetransNode.Scale = new Vector3(0.005f, 0.0052f, 0.005f);
                    break;
                case 2:
                    ObjectModel = (Model)loader.Load("", "p1_wedge");
                    HorsetransNode.Scale = new Vector3(0.002f, 0.002f, 0.002f);
                    break;
                case 3:
                    ObjectModel = (Model)loader.Load("", "3D_Horse_fbx");
                    HorsetransNode.Scale = new Vector3(0.002f, 0.002f, 0.002f);
                    break;
            }
            HorseNode.Model = ObjectModel;
            
        }

        private void KeyBoardToggleObjects()
        {            
           ToggleObjects(toggleObject % 4);
            toggleObject++;
            if (toggleObject > 3)
                toggleObject = 0;
        }

        // Functions for Calculating the Axis and View Matrix
        // Function to create the X,Y and Z axis to be drawn on the Screen
        protected void createAxis(Matrix viewMatrix)
        {
            // Create X Axis from the View Matrix of the Rigth View Port Camera
            xaxis = 2 * viewMatrix.Right;
            xaxis = Vector3.Transform(xaxis, Matrix.CreateTranslation(cam_eye));

            // Create Arrows for X axis in X-Z Plane
            xarrow[0] = 1.8f * viewMatrix.Right;
            Vector3 v = new Vector3(cam_eye.X, cam_eye.Y + 0.2f, cam_eye.Z);
            xarrow[0] = Vector3.Transform(xarrow[0], Matrix.CreateTranslation(v));
            xarrow[1] = 1.8f * viewMatrix.Right;
            v = new Vector3(cam_eye.X, cam_eye.Y - 0.2f, cam_eye.Z);
            xarrow[1] = Vector3.Transform(xarrow[1], Matrix.CreateTranslation(v));
            xarrow[2] = 1.8f * viewMatrix.Right;
            // Create Arrows for X axis in X-Y Plane
            v = new Vector3(cam_eye.X, cam_eye.Y, cam_eye.Z + 0.2f);
            xarrow[2] = Vector3.Transform(xarrow[2], Matrix.CreateTranslation(v));
            xarrow[3] = 1.8f * viewMatrix.Right;
            v = new Vector3(cam_eye.X, cam_eye.Y, cam_eye.Z - 0.2f);
            xarrow[3] = Vector3.Transform(xarrow[3], Matrix.CreateTranslation(v));


            // Create Y Axis from the View Matrix of the Right View Port Camera
            yaxis = 2 * viewMatrix.Up;
            yaxis = Vector3.Transform(yaxis, Matrix.CreateTranslation(cam_eye));

            // Create Arrows for Y Axis in Y-Z Plane
            yarrow[0] = 1.8f * viewMatrix.Up;
            v = new Vector3(cam_eye.X + 0.2f, cam_eye.Y, cam_eye.Z);
            yarrow[0] = Vector3.Transform(yarrow[0], Matrix.CreateTranslation(v));
            yarrow[1] = 1.8f * viewMatrix.Up;
            v = new Vector3(cam_eye.X - 0.2f, cam_eye.Y, cam_eye.Z);
            yarrow[1] = Vector3.Transform(yarrow[1], Matrix.CreateTranslation(v));
            // Create Arrows for Y Axis in X-Y Plane
            yarrow[2] = 1.8f * viewMatrix.Up;
            v = new Vector3(cam_eye.X, cam_eye.Y, cam_eye.Z + 0.2f);
            yarrow[2] = Vector3.Transform(yarrow[2], Matrix.CreateTranslation(v));
            yarrow[3] = 1.8f * viewMatrix.Up;
            v = new Vector3(cam_eye.X, cam_eye.Y, cam_eye.Z - 0.2f);
            yarrow[3] = Vector3.Transform(yarrow[3], Matrix.CreateTranslation(v));

            // Create Z Axis from the View Matrix of the rigth View Camera
            zaxis = (float)distance(cam_eye, cam_center) * viewMatrix.Forward;
            zaxis = Vector3.Transform(zaxis, Matrix.CreateTranslation(cam_eye));

            // Create Arrows for Z Axis in Y-Z Plane
            zarrow[0] = (float)(distance(cam_eye, cam_center) - 0.2f) * viewMatrix.Forward;
            v = new Vector3(cam_eye.X + 0.2f, cam_eye.Y, cam_eye.Z);
            zarrow[0] = Vector3.Transform(zarrow[0], Matrix.CreateTranslation(v));
            zarrow[1] = (float)(distance(cam_eye, cam_center) - 0.2f) * viewMatrix.Forward;
            v = new Vector3(cam_eye.X - 0.2f, cam_eye.Y, cam_eye.Z);
            zarrow[1] = Vector3.Transform(zarrow[1], Matrix.CreateTranslation(v));
            // Create Arrows for Z Axis in X-Z Plane
            zarrow[2] = (float)(distance(cam_eye, cam_center) - 0.2f) * viewMatrix.Forward;
            v = new Vector3(cam_eye.X, cam_eye.Y + 0.2f, cam_eye.Z);
            zarrow[2] = Vector3.Transform(zarrow[2], Matrix.CreateTranslation(v));
            zarrow[3] = (float)(distance(cam_eye, cam_center) - 0.2f) * viewMatrix.Forward;
            v = new Vector3(cam_eye.X, cam_eye.Y - 0.2f, cam_eye.Z);
            zarrow[3] = Vector3.Transform(zarrow[3], Matrix.CreateTranslation(v));


            // Create View Axis from the View Matrix of the Right View Port camera
            vaxis = 2 * viewMatrix.Backward;
            vaxis = Vector3.Transform(vaxis, Matrix.CreateTranslation(cam_eye));

            // Create Arrows for the View Axis( -negative Z Axis) in the Y-Z Plane
            varrow[0] = 1.8f * viewMatrix.Backward;
            v = new Vector3(cam_eye.X + 0.2f, cam_eye.Y, cam_eye.Z);
            varrow[0] = Vector3.Transform(varrow[0], Matrix.CreateTranslation(v));
            varrow[1] = 1.8f * viewMatrix.Backward;
            v = new Vector3(cam_eye.X - 0.2f, cam_eye.Y, cam_eye.Z);
            // Create Arrows for the View Axis ( -negative Z Axis) in the  X-Z Plane
            varrow[1] = Vector3.Transform(varrow[1], Matrix.CreateTranslation(v));
            varrow[2] = 1.8f * viewMatrix.Backward;
            v = new Vector3(cam_eye.X, cam_eye.Y + 0.2f, cam_eye.Z);
            varrow[2] = Vector3.Transform(varrow[2], Matrix.CreateTranslation(v));
            varrow[3] = 1.8f * viewMatrix.Backward;
            v = new Vector3(cam_eye.X, cam_eye.Y - 0.2f, cam_eye.Z);
            varrow[3] = Vector3.Transform(varrow[3], Matrix.CreateTranslation(v));

        }

        private Vector3 CreateTranslationMatrix()
        {
            // Create Translation matrix frmo the Right Camera LookAt value
            Vector3 translation = new Vector3();
            translation.X = cell_lookAt[3].CELLVALUE - cell_lookAt[0].CELLVALUE;
            translation.Y = cell_lookAt[4].CELLVALUE - cell_lookAt[1].CELLVALUE;
            translation.Z = cell_lookAt[5].CELLVALUE - cell_lookAt[2].CELLVALUE;

            return translation;
        }

        private Matrix CreateViewMatrix(Vector3 position, Vector3 LookAt, Vector3 up)
        {
            // Create View Matrix from the right Camera LookAt 
            Matrix rotation = new Matrix();

            rotation.Forward = Vector3.Normalize(LookAt - position);
            rotation.Right = Vector3.Normalize(Vector3.Cross(rotation.Forward, up));
            rotation.Up = Vector3.Normalize(Vector3.Cross(rotation.Right, rotation.Forward));

            return rotation;
        } 

        // Common Functions

        // Calculate Distance Between Two Points
        private double distance(Vector3 point1 , Vector3 point2)
        {
            // calculate Distance between two Vectors
            float x = point1.X - point2.X;
            float y = point1.Y - point2.Y;
            float z = point1.Z - point2.Z;
            return (Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2) + Math.Pow(z, 2)));
        }

        private Vector3 NORMALIZE(Vector3 vToNormalize)
        {
            // Normalize a Vector
            Vector3 v = new Vector3();
            float magnitude = Math.Abs(((float)Math.Pow(vToNormalize.X, 2)) + ((float)Math.Pow(vToNormalize.Y, 2)) + ((float)Math.Pow(vToNormalize.Z, 2)));
            v.X = vToNormalize.X / magnitude;
            v.Y = vToNormalize.Y / magnitude;
            v.Z = vToNormalize.Z / magnitude;

            return v;
        }

        private Vector3 PointForFrustum(Vector3 v1, Vector3 v2)
        {
            Vector3 v = new Vector3();
            v.X = (v1.X - v2.X);
            v.Y = (v1.Y - v2.Y);
            v.Z = (v1.Z - v2.Z);

            return v;
        }

       // Actual Function to calculate the Updation of the Cell Structure in the UI
        // 
        private int selectLocationOfMouse(Point mouseLocation, G2DLabel[] cell_For_Update, int cell_type, int numberOfCells, int bufferForX, int bufferForY)
        {
            // Function to check which cell Label has been selected
            int location = -1;
            int buffer = 6;

            switch (cell_type)
            {
                case 0:
                    buffer = 6;
                    break;
                case 1:
                    buffer = 6;
                    break;
               
            }

            // Selection is Based upon the Mouse Touch Location
            for (int i = 0; i < numberOfCells; i++)
            {
                if ((mouseLocation.X >= (cell_For_Update[i].Bounds.X)) && (mouseLocation.X <= (cell_For_Update[i].Bounds.X + bufferForX))
                 && (mouseLocation.Y >= (cell_For_Update[i].Bounds.Y - (buffer * bufferForY))) && (mouseLocation.Y <= (cell_For_Update[i].Bounds.Y + (buffer * bufferForY))))
                {
                    location = i;
                    break;
                }

            }
            return location;
        }

        private void MouseDragHandlerLight(int button, Point oldmouseLocation, Point newMouseLocation)
        {
            // handler for Mouse Drag Event for the Light and Material Label
            int bufferForX = 40;
            int bufferForY = 10;
           
            if (button == MouseInput.LeftButton)
            {
                // Get the cell ID from the current mouse touch Location
                int cellNum = selectLocationOfMouse(oldmouseLocation, light_direction_bounds, 0,28, bufferForX, bufferForY);
                if (cellNum == -1)
                    return;
                else
                {
                    light_direction[cellNum].TextColor = Color.Red;
                    for (int i = 0; i < 28; i++)
                    {
                        if (i != cellNum)
                            light_direction[i].TextColor = Color.Green;
                    }
                    mouse_motion(oldmouseLocation, newMouseLocation, cell_light[cellNum], 0, light_direction[cellNum]);
                    
                    return;
                }
            }

        }

        private void MouseDragHandlerCamera(int button, Point oldmouseLocation, Point newMouseLocation)
        {
            // handler for the Mouse Drag Event
            int bufferForX = 40;
            int bufferForY = 10;
           
            if (button == MouseInput.LeftButton)
            {
                int cellNum = selectLocationOfMouse(oldmouseLocation, camera_direction_bounds, 1, 9, bufferForX, bufferForY);
                if (cellNum == -1)
                    return;
                else
                {
                    camera_direction[cellNum].TextColor = Color.Red;
                    for (int i = 0; i < 9; i++)
                    {
                        if (i != cellNum)
                            camera_direction[i].TextColor = Color.Green;
                    }
 
                    mouse_motion(oldmouseLocation, newMouseLocation, cell_lookAt[cellNum], 1, camera_direction[cellNum]);
                    
                }
            }


        }

        protected float CalculateCellValue(float min_max_value, float change_of_value, cell light_Cell)
        {
            if (min_max_value >= light_Cell.MAX)
            {
                return light_Cell.MAX;
            }

            else if (min_max_value <= light_Cell.MIN)
            {
                return light_Cell.MIN;
            }

            else if (((min_max_value) > light_Cell.MIN) && ((min_max_value) < light_Cell.MAX))
            {          
                return min_max_value;
            }

            return -10.0f;
        }

        private Vector4 getVectorForupdate(cell cell_for_update, cell[] light_cam, int value, int add)
        {
            Vector4 newUpdateVector = new Vector4();

            // Create New vector for camera 
            if (cell_for_update.ID > 27)
            {
                switch (value)
                {
                    case 0:
                        newUpdateVector = new Vector4((float)cell_for_update.CELLVALUE, (float)light_cam[cell_for_update.ID + add + 1].CELLVALUE, (float)light_cam[cell_for_update.ID + add + 2].CELLVALUE, 0f);
                        break;
                    case 1:
                        newUpdateVector = new Vector4((float)light_cam[cell_for_update.ID + add - 1].CELLVALUE, (float)cell_for_update.CELLVALUE, (float)light_cam[cell_for_update.ID + add + 1].CELLVALUE, 0f);
                        break;
                    case 2:
                        newUpdateVector = new Vector4((float)light_cam[cell_for_update.ID + add - 2].CELLVALUE, (float)light_cam[cell_for_update.ID + add - 1].CELLVALUE, (float)cell_for_update.CELLVALUE, 0f);
                        break;
                }

            }
                // Create new Location for Light and material
            else{
                switch (value)
                {
                    case 0:
                        newUpdateVector = new Vector4((float)cell_for_update.CELLVALUE, (float)light_cam[cell_for_update.ID + add + 1].CELLVALUE, (float)light_cam[cell_for_update.ID + add + 2].CELLVALUE, (float)light_cam[cell_for_update.ID + add + 3].CELLVALUE);
                        break;
                    case 1:
                        newUpdateVector = new Vector4((float)light_cam[cell_for_update.ID + add - 1].CELLVALUE, (float)cell_for_update.CELLVALUE, (float)light_cam[cell_for_update.ID + add + 1].CELLVALUE, (float)light_cam[cell_for_update.ID + add + 2].CELLVALUE);
                        break;
                    case 2:
                        newUpdateVector = new Vector4((float)light_cam[cell_for_update.ID + add - 2].CELLVALUE, (float)light_cam[cell_for_update.ID + add - 1].CELLVALUE, (float)cell_for_update.CELLVALUE, (float)light_cam[cell_for_update.ID + add + 1].CELLVALUE);
                        break;
                    case 3:
                        newUpdateVector = new Vector4((float)light_cam[cell_for_update.ID + add - 3].CELLVALUE, (float)light_cam[cell_for_update.ID + add - 2].CELLVALUE, (float)light_cam[cell_for_update.ID + add - 1].CELLVALUE, (float)cell_for_update.CELLVALUE);
                        break;
                }
            }
            return newUpdateVector;
        }

        private Vector4 vectorForUpdate(int cell_type, cell cell_for_update)
        {
            Vector4 newLightDirection = new Vector4();
            int value = -1;

            if (cell_type == 1)
                value = ((cell_for_update.ID) - 1) % 3;
            else if (cell_type == 0)
                value = (cell_for_update.ID) % 4;
           

            switch (cell_type)
            {
                case 0:
                    newLightDirection = getVectorForupdate(cell_for_update, cell_light, value, 0);
                    break;
                case 1:
                    newLightDirection = getVectorForupdate(cell_for_update, cell_lookAt, value, -28);
                    break;
                
            }

            return newLightDirection;
        }

        protected void mouse_motion(Point OldMouseLoc, Point NewMouseLoc, cell Cell_for_Update, int cell_type, G2DLabel cell_Label)
        {           
            float change_of_value = 0f ;

            if ((maxMousePoint == 0) || (b_maxmousePoint != OldMouseLoc.Y && maxMousePoint != 0))
            {
                b_maxmousePoint = OldMouseLoc.Y;
                maxMousePoint = OldMouseLoc.Y;
            }

            if (((NewMouseLoc.Y > OldMouseLoc.Y ) && ( maxMousePoint < NewMouseLoc.Y))
                || ((NewMouseLoc.Y < OldMouseLoc.Y) && (maxMousePoint < NewMouseLoc.Y)))
            {
                change_of_value = (float)(NewMouseLoc.Y - maxMousePoint);
                maxMousePoint = NewMouseLoc.Y;
            }
            else if (((NewMouseLoc.Y > OldMouseLoc.Y) && (maxMousePoint > NewMouseLoc.Y))
                || ((NewMouseLoc.Y < OldMouseLoc.Y) && (maxMousePoint > NewMouseLoc.Y)))
            {
                change_of_value = -(float)(maxMousePoint - NewMouseLoc.Y);
                maxMousePoint = NewMouseLoc.Y;
            }
            
            Vector4 newVectorUpdate = new Vector4();
            label = Cell_for_Update.INFO;

            Console.WriteLine(":: change of Value :: " + change_of_value);

            float min_max_value = (float)(change_of_value) * (float)Cell_for_Update.STEP  ;
            Console.WriteLine("::  min max Value :: " + min_max_value);

            Cell_for_Update.CELLVALUE = (float)( Cell_for_Update.CELLVALUE +  min_max_value);
            Cell_for_Update.CELLVALUE = (float)(Math.Round(Cell_for_Update.CELLVALUE,2));
            if (Cell_for_Update.CELLVALUE > Cell_for_Update.MAX)
                Cell_for_Update.CELLVALUE = Cell_for_Update.MAX;
            if (Cell_for_Update.CELLVALUE < Cell_for_Update.MIN)
                Cell_for_Update.CELLVALUE = Cell_for_Update.MIN;
                        
           
            if (Cell_for_Update.CELLVALUE == -10.0f)
                return;
            else
            {
                newVectorUpdate = vectorForUpdate(cell_type, Cell_for_Update);
                cell_Label.Text = Cell_for_Update.CELLVALUE.ToString();

                // Update the Light and Material Component
                if (cell_type == 0)
                {
                    cell_light[Cell_for_Update.ID].CELLVALUE = Cell_for_Update.CELLVALUE;
                    for (int i = 0; i < 7; i++)
                    {
                        // Call Light Update
                        if (i < 4)
                        {
                            if ((Cell_for_Update.ID >= i * 4) && (Cell_for_Update.ID < (i * 4 + 4)))
                            {
                                UpdateLights(newVectorUpdate, i);
                                break;
                            }
                        }
                            // Call Material Update
                        else
                        {
                            if ((Cell_for_Update.ID >= i * 4) && (Cell_for_Update.ID < (i * 4 + 4)))
                            {
                                UpdateMaterial(newVectorUpdate, i);
                                break;
                            }
                        }
                    }
                }
                    // Call Camera Update
                else if (cell_type == 1)
                {
                    cell_lookAt[Cell_for_Update.ID - 28].CELLVALUE = Cell_for_Update.CELLVALUE;
                    for (int i = 0; i < 3; i++)
                        if ((Cell_for_Update.ID >= i * 3 + 28) && (Cell_for_Update.ID < (i * 3 + 31)))
                        {
                            UpdateCamera(newVectorUpdate, i);
                            break;
                        }
                }
               
            }
           
        }

        protected override void LoadContent()
        {
            base.LoadContent();           
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            Content.Unload();
        }
        
        protected override void Dispose(bool disposing)
        {
            scene.Dispose();
        }

        protected override void Update(GameTime gameTime)
        {
            scene.Update(gameTime.ElapsedGameTime, gameTime.IsRunningSlowly, this.IsActive);
          
        }

        private void DrawLine(SpriteBatch batch,Vector2 point1, Vector2 point2)
        {
            Texture2D blank = new Texture2D(GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            blank.SetData(new[]{Color.White});

            float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
            float length = Vector2.Distance(point1, point2);
 
            batch.Draw(blank, point1, null, Color.Gray,
              angle, Vector2.Zero, new Vector2(length, 1),
              SpriteEffects.None, 0);
        }

        protected override void Draw(GameTime gameTime)
        {
          // Write the Information of the action performed
            UI2DRenderer.WriteText(new Vector2(50,140), label, Color.Red, tutorialFont);
            UI2DRenderer.WriteText(new Vector2(10,160), "R- ", Color.Red, tutorialFont);
            UI2DRenderer.WriteText(new Vector2(20, 160), " Reset  ", Color.Black, tutorialFont);
            UI2DRenderer.WriteText(new Vector2(70,160), "T- ", Color.Red, tutorialFont);
            UI2DRenderer.WriteText(new Vector2(80, 160), " Toggle  ", Color.Black, tutorialFont);
            UI2DRenderer.WriteText(new Vector2(140,160), "Esc- ", Color.Red, tutorialFont);
            UI2DRenderer.WriteText(new Vector2(165, 160), " Exit  ", Color.Black, tutorialFont);

            // Draw the three axis on 2DUI 
            Point p1 = new Point(50,90);
            Point p2 = new Point(90,90);
            UI2DRenderer.DrawLine(p1, p2, Color.Red, 1);
            UI2DRenderer.DrawLine(p2, new Point(85, 85), Color.Red, 1);
            UI2DRenderer.DrawLine(p2, new Point(85, 97), Color.Red, 1);
            UI2DRenderer.WriteText(new Vector2(90, 70), "x", Color.Black, tutorialFont);

            p2 = new Point(50, 50);
            UI2DRenderer.DrawLine(p1, p2, Color.Green, 1);
            UI2DRenderer.DrawLine(p2,new Point(55,55),Color.Green,1);
            UI2DRenderer.DrawLine(p2, new Point(45,55), Color.Green, 1);
            UI2DRenderer.WriteText(new Vector2(53, 50), "y", Color.Black, tutorialFont);

            p2 = new Point(30, 110);
            UI2DRenderer.DrawLine(p1, p2, Color.Blue, 1);
            UI2DRenderer.DrawLine(p2, new Point(40, 107), Color.Blue, 1);
            UI2DRenderer.DrawLine(p2, new Point(30, 100), Color.Blue, 1);
            UI2DRenderer.WriteText(new Vector2(30, 110), "z", Color.Black, tutorialFont);
   
           
            // Bottom View Port
            scene.SceneRenderTarget = bottomViewRenderTarget;
            scene.BackgroundBound = bottomViewRect;
            scene.CameraNode = bottomCamNode;
            scene.BackgroundColor = Color.CornflowerBlue;
            HorseNode.Parent.Enabled = false;
            camOffset.Parent.Enabled = false;
            planeNearNode1.Parent.Enabled = false;
            planeNearNode2.Parent.Enabled = false;
            planefarNode1.Parent.Enabled = false;
            planefarNode2.Parent.Enabled = false;
            scene.Draw(gameTime.ElapsedGameTime, gameTime.IsRunningSlowly);

          

            // Right View Port
            scene.SceneRenderTarget = rightViewRenderTarget;
            scene.BackgroundBound = rightViewRect;
            scene.CameraNode = rightCamNode;
            scene.BackgroundColor = Color.Black;
            HorseNode.Parent.Enabled = true;
            scene.RenderScene(false, true);

            // Left View Port
            scene.SceneRenderTarget = leftViewRenderTarget;
            scene.BackgroundBound = leftViewRect;
            scene.CameraNode = leftCamNode;
            planeNearNode1.Parent.Enabled = true;
            planeNearNode2.Parent.Enabled = true;
            planefarNode1.Parent.Enabled = true;
            planefarNode2.Parent.Enabled = true;
           
            scene.BackgroundColor = Color.Black;
         
            // Matrix for the Right Cam
            Matrix rightCammatrix = ( Matrix.Identity * rightCam.View * rightCam.Projection);

            // Bounding Frustum of the Right Camera
            BoundingFrustum boundingFrustum = rightCamNode.BoundingFrustum;
            DebugShapeRenderer.AddBoundingFrustum(boundingFrustum, Color.Gray);

            Vector3[] corners = new Vector3[8];
            corners = boundingFrustum.GetCorners();
            
            planeNearTransNode1.Translation = (corners[0] + corners[1] + corners[2] + corners[3]) / 4;
            planeNearTransNode1.Rotation = Quaternion.CreateFromRotationMatrix(viewMatrix);
            planeNearTransNode1.Rotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathHelper.PiOver2);

            planeNearTransNode2.Translation = (corners[0] + corners[1] + corners[2] + corners[3]) / 4;
            planeNearTransNode2.Rotation = Quaternion.CreateFromRotationMatrix(viewMatrix);
            planeNearTransNode2.Rotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitX, -MathHelper.PiOver2);


            planefarTransNode1.Translation = (corners[4] + corners[5] + corners[6] + corners[7]) / 4;
            planefarTransNode1.Rotation = Quaternion.CreateFromRotationMatrix(viewMatrix);
            planefarTransNode1.Rotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathHelper.PiOver2);

            planefarTransNode2.Translation = (corners[4] + corners[5] + corners[6] + corners[7]) / 4;
            planefarTransNode2.Rotation = Quaternion.CreateFromRotationMatrix(viewMatrix);
            planefarTransNode2.Rotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitX, -MathHelper.PiOver2);

             
            // Draws the three Axis 
            DebugShapeRenderer.AddLine(camOffset.Translation, xaxis, Color.Red);
            DebugShapeRenderer.AddTriangle(xaxis, xarrow[0], xarrow[1], Color.Gold);
            DebugShapeRenderer.AddTriangle(xaxis, xarrow[2], xarrow[3], Color.Gold);
            DebugShapeRenderer.AddLine(camOffset.Translation, yaxis, Color.Green);
            DebugShapeRenderer.AddTriangle(yaxis, yarrow[0], yarrow[1], Color.Gold);
            DebugShapeRenderer.AddTriangle(yaxis, yarrow[2], yarrow[3], Color.Gold);
            DebugShapeRenderer.AddLine(camOffset.Translation, zaxis, Color.Gold);
            DebugShapeRenderer.AddTriangle(zaxis, zarrow[0], zarrow[1], Color.Gold);
            DebugShapeRenderer.AddTriangle(zaxis, zarrow[2], zarrow[3], Color.Gold);
            DebugShapeRenderer.AddLine(camOffset.Translation, vaxis, Color.Blue);
            DebugShapeRenderer.AddTriangle(vaxis, varrow[0], varrow[1], Color.Gold);
            DebugShapeRenderer.AddTriangle(vaxis, varrow[2], varrow[3], Color.Gold);

           
            camOffset.Parent.Enabled = true;
            scene.RenderScene(false, true);

  
            State.Device.SetRenderTarget(null);
            // Render the two textures rendered on the render targets
            State.SharedSpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            State.SharedSpriteBatch.Draw(bottomViewRenderTarget, bottomViewRect, Color.White);
            State.SharedSpriteBatch.Draw(leftViewRenderTarget, leftViewRect, Color.White);
            State.SharedSpriteBatch.Draw(rightViewRenderTarget, rightViewRect, Color.White);          
            State.SharedSpriteBatch.End();


            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            
            if ( b_help)
            {
                imageRectangle = new Rectangle(targetLocation.X, targetLocation.Y, 150, 100);
                Rectangle topAdditive = new Rectangle(targetLocation.X, targetLocation.Y - 10, 120, 100);
               
                Vector2 spriteVector ;

                spriteBatch.Draw(image, topAdditive, Color.LightGray);

                for (int i = 0; i < 4; i++)
                {
                     modelObjectRect[i] = new Rectangle(targetLocation.X ,targetLocation.Y + (i *25), 120, 25);
                     spriteVector = new Vector2(targetLocation.X, targetLocation.Y + (i * 25));
                     spriteBatch.Draw(image, modelObjectRect[i], Color.LightGray);
                    if(i != 0)
                     DrawLine(spriteBatch, new Vector2(spriteVector.X + 10 , spriteVector.Y ), new Vector2(spriteVector.X + 110, spriteVector.Y));
                     spriteBatch.DrawString(UIFont, modelObject[i], spriteVector, Color.Black);                    
                }
                                
            }
            spriteBatch.End();
        }
       
    }
}

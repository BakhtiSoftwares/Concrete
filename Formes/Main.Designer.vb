<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Form1
    Inherits System.Windows.Forms.RibbonForm

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Form1))
        Me.Ribbon1 = New System.Windows.Forms.Ribbon()
        Me.NewProjectItem = New System.Windows.Forms.RibbonOrbMenuItem()
        Me.OpenProjectItem = New System.Windows.Forms.RibbonOrbMenuItem()
        Me.SaveProjectItem = New System.Windows.Forms.RibbonOrbMenuItem()
        Me.RibbonSeparator3 = New System.Windows.Forms.RibbonSeparator()
        Me.CalculateMeshItem = New System.Windows.Forms.RibbonOrbMenuItem()
        Me.BroadenItem = New System.Windows.Forms.RibbonOrbMenuItem()
        Me.SqueezeItem = New System.Windows.Forms.RibbonOrbMenuItem()
        Me.RibbonSeparator1 = New System.Windows.Forms.RibbonSeparator()
        Me.CalculateLinearItem = New System.Windows.Forms.RibbonOrbMenuItem()
        Me.CalculateDPMItem = New System.Windows.Forms.RibbonOrbMenuItem()
        Me.RibbonSeparator2 = New System.Windows.Forms.RibbonSeparator()
        Me.ResultsItem = New System.Windows.Forms.RibbonOrbMenuItem()
        Me.CloseButton = New System.Windows.Forms.RibbonOrbOptionButton()
        Me.RibbonOrbRecentItem1 = New System.Windows.Forms.RibbonOrbRecentItem()
        Me.RibbonTab1 = New System.Windows.Forms.RibbonTab()
        Me.RibbonPanel1 = New System.Windows.Forms.RibbonPanel()
        Me.RibbonButton3 = New System.Windows.Forms.RibbonButton()
        Me.RibbonPanel2 = New System.Windows.Forms.RibbonPanel()
        Me.RibbonTextBox1 = New System.Windows.Forms.RibbonTextBox()
        Me.RibbonTextBox2 = New System.Windows.Forms.RibbonTextBox()
        Me.RibbonTextBox6 = New System.Windows.Forms.RibbonTextBox()
        Me.RibbonTextBox3 = New System.Windows.Forms.RibbonTextBox()
        Me.RibbonPanel3 = New System.Windows.Forms.RibbonPanel()
        Me.RibbonButton1 = New System.Windows.Forms.RibbonButton()
        Me.RibbonButton2 = New System.Windows.Forms.RibbonButton()
        Me.RibbonPanel4 = New System.Windows.Forms.RibbonPanel()
        Me.RibbonButton4 = New System.Windows.Forms.RibbonButton()
        Me.RibbonButton5 = New System.Windows.Forms.RibbonButton()
        Me.RibbonButton6 = New System.Windows.Forms.RibbonButton()
        Me.RibbonPanel5 = New System.Windows.Forms.RibbonPanel()
        Me.RibbonButton7 = New System.Windows.Forms.RibbonButton()
        Me.RibbonTab4 = New System.Windows.Forms.RibbonTab()
        Me.RibbonPanel8 = New System.Windows.Forms.RibbonPanel()
        Me.RibbonButton8 = New System.Windows.Forms.RibbonButton()
        Me.RibbonTextBox4 = New System.Windows.Forms.RibbonTextBox()
        Me.RibbonTextBox13 = New System.Windows.Forms.RibbonTextBox()
        Me.RibbonTextBox5 = New System.Windows.Forms.RibbonTextBox()
        Me.RibbonPanel10 = New System.Windows.Forms.RibbonPanel()
        Me.RibbonButton12 = New System.Windows.Forms.RibbonButton()
        Me.RibbonButton9 = New System.Windows.Forms.RibbonButton()
        Me.RibbonTextBox7 = New System.Windows.Forms.RibbonTextBox()
        Me.RibbonTextBox8 = New System.Windows.Forms.RibbonTextBox()
        Me.RibbonTextBox9 = New System.Windows.Forms.RibbonTextBox()
        Me.RibbonTextBox10 = New System.Windows.Forms.RibbonTextBox()
        Me.RibbonTextBox11 = New System.Windows.Forms.RibbonTextBox()
        Me.RibbonTextBox12 = New System.Windows.Forms.RibbonTextBox()
        Me.RibbonPanel9 = New System.Windows.Forms.RibbonPanel()
        Me.RibbonButton13 = New System.Windows.Forms.RibbonButton()
        Me.SplitContainer1 = New System.Windows.Forms.SplitContainer()
        Me.GlControl1 = New OpenTK.GLControl()
        Me.ListBox1 = New System.Windows.Forms.ListBox()
        Me.Timer1 = New System.Windows.Forms.Timer(Me.components)
        Me.RibbonDescriptionMenuItem1 = New System.Windows.Forms.RibbonDescriptionMenuItem()
        Me.RibbonDescriptionMenuItem2 = New System.Windows.Forms.RibbonDescriptionMenuItem()
        CType(Me.SplitContainer1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SplitContainer1.Panel1.SuspendLayout()
        Me.SplitContainer1.Panel2.SuspendLayout()
        Me.SplitContainer1.SuspendLayout()
        Me.SuspendLayout()
        '
        'Ribbon1
        '
        Me.Ribbon1.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.Ribbon1.Location = New System.Drawing.Point(0, 0)
        Me.Ribbon1.Minimized = False
        Me.Ribbon1.Name = "Ribbon1"
        '
        '
        '
        Me.Ribbon1.OrbDropDown.BorderRoundness = 2
        Me.Ribbon1.OrbDropDown.Location = New System.Drawing.Point(0, 0)
        Me.Ribbon1.OrbDropDown.MenuItems.Add(Me.NewProjectItem)
        Me.Ribbon1.OrbDropDown.MenuItems.Add(Me.OpenProjectItem)
        Me.Ribbon1.OrbDropDown.MenuItems.Add(Me.SaveProjectItem)
        Me.Ribbon1.OrbDropDown.MenuItems.Add(Me.RibbonSeparator3)
        Me.Ribbon1.OrbDropDown.MenuItems.Add(Me.CalculateMeshItem)
        Me.Ribbon1.OrbDropDown.MenuItems.Add(Me.BroadenItem)
        Me.Ribbon1.OrbDropDown.MenuItems.Add(Me.SqueezeItem)
        Me.Ribbon1.OrbDropDown.MenuItems.Add(Me.RibbonSeparator1)
        Me.Ribbon1.OrbDropDown.MenuItems.Add(Me.CalculateLinearItem)
        Me.Ribbon1.OrbDropDown.MenuItems.Add(Me.CalculateDPMItem)
        Me.Ribbon1.OrbDropDown.MenuItems.Add(Me.RibbonSeparator2)
        Me.Ribbon1.OrbDropDown.MenuItems.Add(Me.ResultsItem)
        Me.Ribbon1.OrbDropDown.Name = ""
        Me.Ribbon1.OrbDropDown.OptionItems.Add(Me.CloseButton)
        Me.Ribbon1.OrbDropDown.RecentItems.Add(Me.RibbonOrbRecentItem1)
        Me.Ribbon1.OrbDropDown.Size = New System.Drawing.Size(527, 477)
        Me.Ribbon1.OrbDropDown.TabIndex = 0
        Me.Ribbon1.OrbStyle = System.Windows.Forms.RibbonOrbStyle.Office_2013
        Me.Ribbon1.OrbText = "Concrete"
        Me.Ribbon1.RibbonTabFont = New System.Drawing.Font("Trebuchet MS", 9.0!)
        Me.Ribbon1.Size = New System.Drawing.Size(1350, 144)
        Me.Ribbon1.TabIndex = 0
        Me.Ribbon1.Tabs.Add(Me.RibbonTab1)
        Me.Ribbon1.Tabs.Add(Me.RibbonTab4)
        Me.Ribbon1.TabSpacing = 4
        Me.Ribbon1.Text = "Ribbon1"
        '
        'NewProjectItem
        '
        Me.NewProjectItem.DropDownArrowDirection = System.Windows.Forms.RibbonArrowDirection.Left
        Me.NewProjectItem.Image = CType(resources.GetObject("NewProjectItem.Image"), System.Drawing.Image)
        Me.NewProjectItem.LargeImage = CType(resources.GetObject("NewProjectItem.LargeImage"), System.Drawing.Image)
        Me.NewProjectItem.Name = "NewProjectItem"
        Me.NewProjectItem.SmallImage = CType(resources.GetObject("NewProjectItem.SmallImage"), System.Drawing.Image)
        Me.NewProjectItem.Text = "New"
        '
        'OpenProjectItem
        '
        Me.OpenProjectItem.DropDownArrowDirection = System.Windows.Forms.RibbonArrowDirection.Left
        Me.OpenProjectItem.Image = CType(resources.GetObject("OpenProjectItem.Image"), System.Drawing.Image)
        Me.OpenProjectItem.LargeImage = CType(resources.GetObject("OpenProjectItem.LargeImage"), System.Drawing.Image)
        Me.OpenProjectItem.Name = "OpenProjectItem"
        Me.OpenProjectItem.SmallImage = CType(resources.GetObject("OpenProjectItem.SmallImage"), System.Drawing.Image)
        Me.OpenProjectItem.Text = "Open"
        '
        'SaveProjectItem
        '
        Me.SaveProjectItem.DropDownArrowDirection = System.Windows.Forms.RibbonArrowDirection.Left
        Me.SaveProjectItem.Image = CType(resources.GetObject("SaveProjectItem.Image"), System.Drawing.Image)
        Me.SaveProjectItem.LargeImage = CType(resources.GetObject("SaveProjectItem.LargeImage"), System.Drawing.Image)
        Me.SaveProjectItem.Name = "SaveProjectItem"
        Me.SaveProjectItem.SmallImage = CType(resources.GetObject("SaveProjectItem.SmallImage"), System.Drawing.Image)
        Me.SaveProjectItem.Text = "Save"
        '
        'RibbonSeparator3
        '
        Me.RibbonSeparator3.Name = "RibbonSeparator3"
        '
        'CalculateMeshItem
        '
        Me.CalculateMeshItem.DropDownArrowDirection = System.Windows.Forms.RibbonArrowDirection.Left
        Me.CalculateMeshItem.Image = CType(resources.GetObject("CalculateMeshItem.Image"), System.Drawing.Image)
        Me.CalculateMeshItem.LargeImage = CType(resources.GetObject("CalculateMeshItem.LargeImage"), System.Drawing.Image)
        Me.CalculateMeshItem.Name = "CalculateMeshItem"
        Me.CalculateMeshItem.SmallImage = CType(resources.GetObject("CalculateMeshItem.SmallImage"), System.Drawing.Image)
        Me.CalculateMeshItem.Text = "Calculate Mesh"
        '
        'BroadenItem
        '
        Me.BroadenItem.DropDownArrowDirection = System.Windows.Forms.RibbonArrowDirection.Left
        Me.BroadenItem.Image = CType(resources.GetObject("BroadenItem.Image"), System.Drawing.Image)
        Me.BroadenItem.LargeImage = CType(resources.GetObject("BroadenItem.LargeImage"), System.Drawing.Image)
        Me.BroadenItem.Name = "BroadenItem"
        Me.BroadenItem.SmallImage = CType(resources.GetObject("BroadenItem.SmallImage"), System.Drawing.Image)
        Me.BroadenItem.Text = "Broaden"
        '
        'SqueezeItem
        '
        Me.SqueezeItem.DropDownArrowDirection = System.Windows.Forms.RibbonArrowDirection.Left
        Me.SqueezeItem.Image = CType(resources.GetObject("SqueezeItem.Image"), System.Drawing.Image)
        Me.SqueezeItem.LargeImage = CType(resources.GetObject("SqueezeItem.LargeImage"), System.Drawing.Image)
        Me.SqueezeItem.Name = "SqueezeItem"
        Me.SqueezeItem.SmallImage = CType(resources.GetObject("SqueezeItem.SmallImage"), System.Drawing.Image)
        Me.SqueezeItem.Text = "Squeeze"
        '
        'RibbonSeparator1
        '
        Me.RibbonSeparator1.Name = "RibbonSeparator1"
        '
        'CalculateLinearItem
        '
        Me.CalculateLinearItem.DropDownArrowDirection = System.Windows.Forms.RibbonArrowDirection.Left
        Me.CalculateLinearItem.Image = CType(resources.GetObject("CalculateLinearItem.Image"), System.Drawing.Image)
        Me.CalculateLinearItem.LargeImage = CType(resources.GetObject("CalculateLinearItem.LargeImage"), System.Drawing.Image)
        Me.CalculateLinearItem.Name = "CalculateLinearItem"
        Me.CalculateLinearItem.SmallImage = CType(resources.GetObject("CalculateLinearItem.SmallImage"), System.Drawing.Image)
        Me.CalculateLinearItem.Text = "Calculate Linear"
        '
        'CalculateDPMItem
        '
        Me.CalculateDPMItem.DropDownArrowDirection = System.Windows.Forms.RibbonArrowDirection.Left
        Me.CalculateDPMItem.Image = CType(resources.GetObject("CalculateDPMItem.Image"), System.Drawing.Image)
        Me.CalculateDPMItem.LargeImage = CType(resources.GetObject("CalculateDPMItem.LargeImage"), System.Drawing.Image)
        Me.CalculateDPMItem.Name = "CalculateDPMItem"
        Me.CalculateDPMItem.SmallImage = CType(resources.GetObject("CalculateDPMItem.SmallImage"), System.Drawing.Image)
        Me.CalculateDPMItem.Text = "Calculate DPM"
        '
        'RibbonSeparator2
        '
        Me.RibbonSeparator2.Name = "RibbonSeparator2"
        '
        'ResultsItem
        '
        Me.ResultsItem.DropDownArrowDirection = System.Windows.Forms.RibbonArrowDirection.Left
        Me.ResultsItem.Image = CType(resources.GetObject("ResultsItem.Image"), System.Drawing.Image)
        Me.ResultsItem.LargeImage = CType(resources.GetObject("ResultsItem.LargeImage"), System.Drawing.Image)
        Me.ResultsItem.Name = "ResultsItem"
        Me.ResultsItem.SmallImage = CType(resources.GetObject("ResultsItem.SmallImage"), System.Drawing.Image)
        Me.ResultsItem.Text = "Results"
        '
        'CloseButton
        '
        Me.CloseButton.Image = CType(resources.GetObject("CloseButton.Image"), System.Drawing.Image)
        Me.CloseButton.LargeImage = CType(resources.GetObject("CloseButton.LargeImage"), System.Drawing.Image)
        Me.CloseButton.Name = "CloseButton"
        Me.CloseButton.SmallImage = CType(resources.GetObject("CloseButton.SmallImage"), System.Drawing.Image)
        Me.CloseButton.Text = "Close"
        '
        'RibbonOrbRecentItem1
        '
        Me.RibbonOrbRecentItem1.Image = CType(resources.GetObject("RibbonOrbRecentItem1.Image"), System.Drawing.Image)
        Me.RibbonOrbRecentItem1.LargeImage = CType(resources.GetObject("RibbonOrbRecentItem1.LargeImage"), System.Drawing.Image)
        Me.RibbonOrbRecentItem1.Name = "RibbonOrbRecentItem1"
        Me.RibbonOrbRecentItem1.SmallImage = CType(resources.GetObject("RibbonOrbRecentItem1.SmallImage"), System.Drawing.Image)
        '
        'RibbonTab1
        '
        Me.RibbonTab1.Name = "RibbonTab1"
        Me.RibbonTab1.Panels.Add(Me.RibbonPanel1)
        Me.RibbonTab1.Panels.Add(Me.RibbonPanel2)
        Me.RibbonTab1.Panels.Add(Me.RibbonPanel3)
        Me.RibbonTab1.Panels.Add(Me.RibbonPanel4)
        Me.RibbonTab1.Panels.Add(Me.RibbonPanel5)
        Me.RibbonTab1.Text = "Geometry"
        '
        'RibbonPanel1
        '
        Me.RibbonPanel1.Items.Add(Me.RibbonButton3)
        Me.RibbonPanel1.Name = "RibbonPanel1"
        Me.RibbonPanel1.Text = "Type"
        '
        'RibbonButton3
        '
        Me.RibbonButton3.Image = CType(resources.GetObject("RibbonButton3.Image"), System.Drawing.Image)
        Me.RibbonButton3.LargeImage = CType(resources.GetObject("RibbonButton3.LargeImage"), System.Drawing.Image)
        Me.RibbonButton3.Name = "RibbonButton3"
        Me.RibbonButton3.SmallImage = CType(resources.GetObject("RibbonButton3.SmallImage"), System.Drawing.Image)
        Me.RibbonButton3.Style = System.Windows.Forms.RibbonButtonStyle.SplitDropDown
        Me.RibbonButton3.Text = "RibbonButton3"
        '
        'RibbonPanel2
        '
        Me.RibbonPanel2.FlowsTo = System.Windows.Forms.RibbonPanelFlowDirection.Right
        Me.RibbonPanel2.Items.Add(Me.RibbonTextBox1)
        Me.RibbonPanel2.Items.Add(Me.RibbonTextBox2)
        Me.RibbonPanel2.Items.Add(Me.RibbonTextBox6)
        Me.RibbonPanel2.Items.Add(Me.RibbonTextBox3)
        Me.RibbonPanel2.Name = "RibbonPanel2"
        Me.RibbonPanel2.Text = "Dimensions"
        '
        'RibbonTextBox1
        '
        Me.RibbonTextBox1.LabelWidth = 50
        Me.RibbonTextBox1.Name = "RibbonTextBox1"
        Me.RibbonTextBox1.Text = "RibbonTextBox1"
        Me.RibbonTextBox1.TextBoxText = "100"
        '
        'RibbonTextBox2
        '
        Me.RibbonTextBox2.LabelWidth = 50
        Me.RibbonTextBox2.Name = "RibbonTextBox2"
        Me.RibbonTextBox2.Text = "RibbonTextBox2"
        Me.RibbonTextBox2.TextBoxText = "100"
        '
        'RibbonTextBox6
        '
        Me.RibbonTextBox6.LabelWidth = 50
        Me.RibbonTextBox6.Name = "RibbonTextBox6"
        Me.RibbonTextBox6.Text = "gg"
        Me.RibbonTextBox6.TextBoxText = "100"
        '
        'RibbonTextBox3
        '
        Me.RibbonTextBox3.Name = "RibbonTextBox3"
        Me.RibbonTextBox3.TextBoxText = "30"
        '
        'RibbonPanel3
        '
        Me.RibbonPanel3.Items.Add(Me.RibbonButton1)
        Me.RibbonPanel3.Items.Add(Me.RibbonButton2)
        Me.RibbonPanel3.Name = "RibbonPanel3"
        Me.RibbonPanel3.Text = "Confinement"
        '
        'RibbonButton1
        '
        Me.RibbonButton1.Image = CType(resources.GetObject("RibbonButton1.Image"), System.Drawing.Image)
        Me.RibbonButton1.LargeImage = CType(resources.GetObject("RibbonButton1.LargeImage"), System.Drawing.Image)
        Me.RibbonButton1.Name = "RibbonButton1"
        Me.RibbonButton1.SmallImage = CType(resources.GetObject("RibbonButton1.SmallImage"), System.Drawing.Image)
        Me.RibbonButton1.Text = "Add"
        '
        'RibbonButton2
        '
        Me.RibbonButton2.Image = CType(resources.GetObject("RibbonButton2.Image"), System.Drawing.Image)
        Me.RibbonButton2.LargeImage = CType(resources.GetObject("RibbonButton2.LargeImage"), System.Drawing.Image)
        Me.RibbonButton2.Name = "RibbonButton2"
        Me.RibbonButton2.SmallImage = CType(resources.GetObject("RibbonButton2.SmallImage"), System.Drawing.Image)
        Me.RibbonButton2.Text = "Delete"
        '
        'RibbonPanel4
        '
        Me.RibbonPanel4.Items.Add(Me.RibbonButton4)
        Me.RibbonPanel4.Items.Add(Me.RibbonButton5)
        Me.RibbonPanel4.Items.Add(Me.RibbonButton6)
        Me.RibbonPanel4.Name = "RibbonPanel4"
        Me.RibbonPanel4.Text = "Mesh"
        '
        'RibbonButton4
        '
        Me.RibbonButton4.Image = CType(resources.GetObject("RibbonButton4.Image"), System.Drawing.Image)
        Me.RibbonButton4.LargeImage = CType(resources.GetObject("RibbonButton4.LargeImage"), System.Drawing.Image)
        Me.RibbonButton4.Name = "RibbonButton4"
        Me.RibbonButton4.SmallImage = CType(resources.GetObject("RibbonButton4.SmallImage"), System.Drawing.Image)
        Me.RibbonButton4.Text = "calculate"
        '
        'RibbonButton5
        '
        Me.RibbonButton5.Image = CType(resources.GetObject("RibbonButton5.Image"), System.Drawing.Image)
        Me.RibbonButton5.LargeImage = CType(resources.GetObject("RibbonButton5.LargeImage"), System.Drawing.Image)
        Me.RibbonButton5.Name = "RibbonButton5"
        Me.RibbonButton5.SmallImage = CType(resources.GetObject("RibbonButton5.SmallImage"), System.Drawing.Image)
        Me.RibbonButton5.Text = "Broaden"
        '
        'RibbonButton6
        '
        Me.RibbonButton6.Image = CType(resources.GetObject("RibbonButton6.Image"), System.Drawing.Image)
        Me.RibbonButton6.LargeImage = CType(resources.GetObject("RibbonButton6.LargeImage"), System.Drawing.Image)
        Me.RibbonButton6.Name = "RibbonButton6"
        Me.RibbonButton6.SmallImage = CType(resources.GetObject("RibbonButton6.SmallImage"), System.Drawing.Image)
        Me.RibbonButton6.Text = "Squeeze"
        '
        'RibbonPanel5
        '
        Me.RibbonPanel5.Items.Add(Me.RibbonButton7)
        Me.RibbonPanel5.Name = "RibbonPanel5"
        Me.RibbonPanel5.Text = "Drawing"
        '
        'RibbonButton7
        '
        Me.RibbonButton7.Image = CType(resources.GetObject("RibbonButton7.Image"), System.Drawing.Image)
        Me.RibbonButton7.LargeImage = CType(resources.GetObject("RibbonButton7.LargeImage"), System.Drawing.Image)
        Me.RibbonButton7.Name = "RibbonButton7"
        Me.RibbonButton7.SmallImage = CType(resources.GetObject("RibbonButton7.SmallImage"), System.Drawing.Image)
        Me.RibbonButton7.Style = System.Windows.Forms.RibbonButtonStyle.SplitDropDown
        Me.RibbonButton7.Text = "RibbonButton7"
        '
        'RibbonTab4
        '
        Me.RibbonTab4.Name = "RibbonTab4"
        Me.RibbonTab4.Panels.Add(Me.RibbonPanel8)
        Me.RibbonTab4.Panels.Add(Me.RibbonPanel10)
        Me.RibbonTab4.Panels.Add(Me.RibbonPanel9)
        Me.RibbonTab4.Text = "Calculation"
        '
        'RibbonPanel8
        '
        Me.RibbonPanel8.Items.Add(Me.RibbonButton8)
        Me.RibbonPanel8.Items.Add(Me.RibbonTextBox4)
        Me.RibbonPanel8.Items.Add(Me.RibbonTextBox13)
        Me.RibbonPanel8.Items.Add(Me.RibbonTextBox5)
        Me.RibbonPanel8.Name = "RibbonPanel8"
        Me.RibbonPanel8.Text = "Linear"
        '
        'RibbonButton8
        '
        Me.RibbonButton8.Image = CType(resources.GetObject("RibbonButton8.Image"), System.Drawing.Image)
        Me.RibbonButton8.LargeImage = CType(resources.GetObject("RibbonButton8.LargeImage"), System.Drawing.Image)
        Me.RibbonButton8.MinimumSize = New System.Drawing.Size(70, 0)
        Me.RibbonButton8.Name = "RibbonButton8"
        Me.RibbonButton8.SmallImage = CType(resources.GetObject("RibbonButton8.SmallImage"), System.Drawing.Image)
        Me.RibbonButton8.Text = "Calculate"
        '
        'RibbonTextBox4
        '
        Me.RibbonTextBox4.LabelWidth = 60
        Me.RibbonTextBox4.MaxSizeMode = System.Windows.Forms.RibbonElementSizeMode.Large
        Me.RibbonTextBox4.Name = "RibbonTextBox4"
        Me.RibbonTextBox4.Text = "E (MPa)"
        Me.RibbonTextBox4.TextBoxText = "26000"
        Me.RibbonTextBox4.TextBoxWidth = 60
        '
        'RibbonTextBox13
        '
        Me.RibbonTextBox13.LabelWidth = 60
        Me.RibbonTextBox13.MaxSizeMode = System.Windows.Forms.RibbonElementSizeMode.Large
        Me.RibbonTextBox13.Name = "RibbonTextBox13"
        Me.RibbonTextBox13.Text = "ν"
        Me.RibbonTextBox13.TextBoxText = "0.2"
        Me.RibbonTextBox13.TextBoxWidth = 60
        '
        'RibbonTextBox5
        '
        Me.RibbonTextBox5.LabelWidth = 60
        Me.RibbonTextBox5.Name = "RibbonTextBox5"
        Me.RibbonTextBox5.Text = "F (MPa)"
        Me.RibbonTextBox5.TextBoxText = "-25"
        Me.RibbonTextBox5.TextBoxWidth = 60
        '
        'RibbonPanel10
        '
        Me.RibbonPanel10.Items.Add(Me.RibbonButton12)
        Me.RibbonPanel10.Items.Add(Me.RibbonButton9)
        Me.RibbonPanel10.Items.Add(Me.RibbonTextBox7)
        Me.RibbonPanel10.Items.Add(Me.RibbonTextBox8)
        Me.RibbonPanel10.Items.Add(Me.RibbonTextBox9)
        Me.RibbonPanel10.Items.Add(Me.RibbonTextBox10)
        Me.RibbonPanel10.Items.Add(Me.RibbonTextBox11)
        Me.RibbonPanel10.Items.Add(Me.RibbonTextBox12)
        Me.RibbonPanel10.Name = "RibbonPanel10"
        Me.RibbonPanel10.Text = "Plastic Damage Model "
        '
        'RibbonButton12
        '
        Me.RibbonButton12.Image = CType(resources.GetObject("RibbonButton12.Image"), System.Drawing.Image)
        Me.RibbonButton12.LargeImage = CType(resources.GetObject("RibbonButton12.LargeImage"), System.Drawing.Image)
        Me.RibbonButton12.MinimumSize = New System.Drawing.Size(80, 0)
        Me.RibbonButton12.Name = "RibbonButton12"
        Me.RibbonButton12.SmallImage = CType(resources.GetObject("RibbonButton12.SmallImage"), System.Drawing.Image)
        Me.RibbonButton12.Text = "Calculate"
        '
        'RibbonButton9
        '
        Me.RibbonButton9.Image = Global.Doctorat.My.Resources.Resources.switchOn
        Me.RibbonButton9.LargeImage = Global.Doctorat.My.Resources.Resources.switchOn
        Me.RibbonButton9.MinimumSize = New System.Drawing.Size(70, 0)
        Me.RibbonButton9.Name = "RibbonButton9"
        Me.RibbonButton9.SmallImage = Global.Doctorat.My.Resources.Resources.switchOn
        Me.RibbonButton9.Text = "Compression"
        '
        'RibbonTextBox7
        '
        Me.RibbonTextBox7.LabelWidth = 80
        Me.RibbonTextBox7.Name = "RibbonTextBox7"
        Me.RibbonTextBox7.Text = "Fb0/Fc0"
        Me.RibbonTextBox7.TextBoxText = "1.16"
        Me.RibbonTextBox7.TextBoxWidth = 50
        '
        'RibbonTextBox8
        '
        Me.RibbonTextBox8.LabelWidth = 80
        Me.RibbonTextBox8.Name = "RibbonTextBox8"
        Me.RibbonTextBox8.Text = "Dilation angle"
        Me.RibbonTextBox8.TextBoxText = "5"
        Me.RibbonTextBox8.TextBoxWidth = 50
        '
        'RibbonTextBox9
        '
        Me.RibbonTextBox9.LabelWidth = 80
        Me.RibbonTextBox9.Name = "RibbonTextBox9"
        Me.RibbonTextBox9.Text = "Eccentricity "
        Me.RibbonTextBox9.TextBoxText = "0.1"
        Me.RibbonTextBox9.TextBoxWidth = 50
        '
        'RibbonTextBox10
        '
        Me.RibbonTextBox10.LabelWidth = 70
        Me.RibbonTextBox10.Name = "RibbonTextBox10"
        Me.RibbonTextBox10.Text = "K"
        Me.RibbonTextBox10.TextBoxText = "0.67"
        Me.RibbonTextBox10.TextBoxWidth = 50
        '
        'RibbonTextBox11
        '
        Me.RibbonTextBox11.LabelWidth = 70
        Me.RibbonTextBox11.Name = "RibbonTextBox11"
        Me.RibbonTextBox11.Text = "Fcm (MPa)"
        Me.RibbonTextBox11.TextBoxText = "25"
        Me.RibbonTextBox11.TextBoxWidth = 50
        '
        'RibbonTextBox12
        '
        Me.RibbonTextBox12.LabelWidth = 70
        Me.RibbonTextBox12.Name = "RibbonTextBox12"
        Me.RibbonTextBox12.Text = "ν"
        Me.RibbonTextBox12.TextBoxText = "0.2"
        Me.RibbonTextBox12.TextBoxWidth = 50
        '
        'RibbonPanel9
        '
        Me.RibbonPanel9.Items.Add(Me.RibbonButton13)
        Me.RibbonPanel9.Name = "RibbonPanel9"
        Me.RibbonPanel9.Text = "Draw Curves"
        '
        'RibbonButton13
        '
        Me.RibbonButton13.Image = CType(resources.GetObject("RibbonButton13.Image"), System.Drawing.Image)
        Me.RibbonButton13.LargeImage = CType(resources.GetObject("RibbonButton13.LargeImage"), System.Drawing.Image)
        Me.RibbonButton13.Name = "RibbonButton13"
        Me.RibbonButton13.SmallImage = CType(resources.GetObject("RibbonButton13.SmallImage"), System.Drawing.Image)
        Me.RibbonButton13.Text = "Draw"
        '
        'SplitContainer1
        '
        Me.SplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.SplitContainer1.Location = New System.Drawing.Point(0, 144)
        Me.SplitContainer1.Name = "SplitContainer1"
        Me.SplitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal
        '
        'SplitContainer1.Panel1
        '
        Me.SplitContainer1.Panel1.Controls.Add(Me.GlControl1)
        '
        'SplitContainer1.Panel2
        '
        Me.SplitContainer1.Panel2.Controls.Add(Me.ListBox1)
        Me.SplitContainer1.Panel2.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.SplitContainer1.Size = New System.Drawing.Size(1350, 585)
        Me.SplitContainer1.SplitterDistance = 330
        Me.SplitContainer1.TabIndex = 1
        '
        'GlControl1
        '
        Me.GlControl1.AutoScroll = True
        Me.GlControl1.AutoValidate = System.Windows.Forms.AutoValidate.EnablePreventFocusChange
        Me.GlControl1.BackColor = System.Drawing.Color.Maroon
        Me.GlControl1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.GlControl1.Cursor = System.Windows.Forms.Cursors.Cross
        Me.GlControl1.Location = New System.Drawing.Point(597, 219)
        Me.GlControl1.Name = "GlControl1"
        Me.GlControl1.Size = New System.Drawing.Size(157, 103)
        Me.GlControl1.TabIndex = 6
        Me.GlControl1.VSync = True
        '
        'ListBox1
        '
        Me.ListBox1.Font = New System.Drawing.Font("Microsoft Sans Serif", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ListBox1.FormattingEnabled = True
        Me.ListBox1.ItemHeight = 24
        Me.ListBox1.Location = New System.Drawing.Point(124, 22)
        Me.ListBox1.Name = "ListBox1"
        Me.ListBox1.Size = New System.Drawing.Size(964, 76)
        Me.ListBox1.TabIndex = 0
        '
        'Timer1
        '
        Me.Timer1.Enabled = True
        Me.Timer1.Interval = 1000
        '
        'RibbonDescriptionMenuItem1
        '
        Me.RibbonDescriptionMenuItem1.DescriptionBounds = New System.Drawing.Rectangle(0, 0, 0, 0)
        Me.RibbonDescriptionMenuItem1.DropDownArrowDirection = System.Windows.Forms.RibbonArrowDirection.Left
        Me.RibbonDescriptionMenuItem1.Image = CType(resources.GetObject("RibbonDescriptionMenuItem1.Image"), System.Drawing.Image)
        Me.RibbonDescriptionMenuItem1.LargeImage = CType(resources.GetObject("RibbonDescriptionMenuItem1.LargeImage"), System.Drawing.Image)
        Me.RibbonDescriptionMenuItem1.Name = "RibbonDescriptionMenuItem1"
        Me.RibbonDescriptionMenuItem1.SmallImage = CType(resources.GetObject("RibbonDescriptionMenuItem1.SmallImage"), System.Drawing.Image)
        Me.RibbonDescriptionMenuItem1.Text = "RibbonDescriptionMenuItem1"
        '
        'RibbonDescriptionMenuItem2
        '
        Me.RibbonDescriptionMenuItem2.DescriptionBounds = New System.Drawing.Rectangle(0, 0, 0, 0)
        Me.RibbonDescriptionMenuItem2.DropDownArrowDirection = System.Windows.Forms.RibbonArrowDirection.Left
        Me.RibbonDescriptionMenuItem2.Image = CType(resources.GetObject("RibbonDescriptionMenuItem2.Image"), System.Drawing.Image)
        Me.RibbonDescriptionMenuItem2.LargeImage = CType(resources.GetObject("RibbonDescriptionMenuItem2.LargeImage"), System.Drawing.Image)
        Me.RibbonDescriptionMenuItem2.Name = "RibbonDescriptionMenuItem2"
        Me.RibbonDescriptionMenuItem2.SmallImage = CType(resources.GetObject("RibbonDescriptionMenuItem2.SmallImage"), System.Drawing.Image)
        Me.RibbonDescriptionMenuItem2.Text = "RibbonDescriptionMenuItem2"
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1350, 729)
        Me.Controls.Add(Me.SplitContainer1)
        Me.Controls.Add(Me.Ribbon1)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.KeyPreview = True
        Me.Name = "Form1"
        Me.Text = "Concrete v2.0.0"
        Me.SplitContainer1.Panel1.ResumeLayout(False)
        Me.SplitContainer1.Panel2.ResumeLayout(False)
        CType(Me.SplitContainer1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.SplitContainer1.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents Ribbon1 As Ribbon
    Friend WithEvents SplitContainer1 As SplitContainer
    Friend WithEvents GlControl1 As OpenTK.GLControl
    Friend WithEvents ListBox1 As ListBox
    Friend WithEvents RibbonTab1 As RibbonTab
    Friend WithEvents RibbonTab2 As RibbonTab
    Friend WithEvents RibbonTab3 As RibbonTab
    Friend WithEvents RibbonTab4 As RibbonTab
    Friend WithEvents RibbonPanel1 As RibbonPanel
    Friend WithEvents RibbonPanel2 As RibbonPanel
    Friend WithEvents RibbonPanel3 As RibbonPanel
    Friend WithEvents RibbonPanel4 As RibbonPanel
    Friend WithEvents RibbonTextBox1 As RibbonTextBox
    Friend WithEvents RibbonTextBox2 As RibbonTextBox
    Friend WithEvents RibbonButton1 As RibbonButton
    Friend WithEvents RibbonButton2 As RibbonButton
    Friend WithEvents RibbonButton3 As RibbonButton
    Friend WithEvents RibbonButton4 As RibbonButton
    Friend WithEvents RibbonButton5 As RibbonButton
    Friend WithEvents RibbonButton6 As RibbonButton
    Friend WithEvents Timer1 As Timer
    Friend WithEvents RibbonPanel5 As RibbonPanel
    Friend WithEvents RibbonButton7 As RibbonButton
    Friend WithEvents RibbonPanel6 As RibbonPanel
    Friend WithEvents RibbonPanel7 As RibbonPanel
    Friend WithEvents RibbonPanel8 As RibbonPanel
    Friend WithEvents RibbonButton8 As RibbonButton
    Friend WithEvents RibbonTextBox6 As RibbonTextBox
    Friend WithEvents RibbonButton12 As RibbonButton
    Friend WithEvents RibbonTextBox3 As RibbonTextBox
    Friend WithEvents RibbonPanel9 As RibbonPanel
    Friend WithEvents RibbonButton13 As RibbonButton
    Friend WithEvents RibbonPanel10 As RibbonPanel
    Friend WithEvents RibbonTextBox4 As RibbonTextBox
    Friend WithEvents RibbonTextBox7 As RibbonTextBox
    Friend WithEvents RibbonTextBox8 As RibbonTextBox
    Friend WithEvents RibbonTextBox9 As RibbonTextBox
    Friend WithEvents RibbonTextBox10 As RibbonTextBox
    Friend WithEvents RibbonTextBox11 As RibbonTextBox
    Friend WithEvents RibbonTextBox12 As RibbonTextBox
    Friend WithEvents RibbonTextBox13 As RibbonTextBox
    Friend WithEvents RibbonDescriptionMenuItem1 As RibbonDescriptionMenuItem
    Friend WithEvents RibbonDescriptionMenuItem2 As RibbonDescriptionMenuItem
    Friend WithEvents RibbonTextBox5 As RibbonTextBox
    Friend WithEvents NewProjectItem As RibbonOrbMenuItem
    Friend WithEvents OpenProjectItem As RibbonOrbMenuItem
    Friend WithEvents SaveProjectItem As RibbonOrbMenuItem
    Friend WithEvents RibbonSeparator1 As RibbonSeparator
    Friend WithEvents CalculateLinearItem As RibbonOrbMenuItem
    Friend WithEvents CalculateDPMItem As RibbonOrbMenuItem
    Friend WithEvents RibbonSeparator2 As RibbonSeparator
    Friend WithEvents ResultsItem As RibbonOrbMenuItem
    Friend WithEvents CloseButton As RibbonOrbOptionButton
    Friend WithEvents RibbonSeparator3 As RibbonSeparator
    Friend WithEvents CalculateMeshItem As RibbonOrbMenuItem
    Friend WithEvents BroadenItem As RibbonOrbMenuItem
    Friend WithEvents SqueezeItem As RibbonOrbMenuItem
    Friend WithEvents RibbonOrbOptionButton1 As RibbonOrbOptionButton
    Friend WithEvents RibbonOrbRecentItem1 As RibbonOrbRecentItem
    Friend WithEvents RibbonButton9 As RibbonButton
End Class

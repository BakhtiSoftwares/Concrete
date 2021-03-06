Imports OpenTK
Imports OpenTK.Platform
Imports OpenTK.Graphics.OpenGL
Imports System.Math
Imports Doctorat.Base
Imports OOFEMcli
Public Class Form1
    Structure ConcreteData
        Dim TextRib1, TextRib2, TextRib3, TextRib4, TextRib5, TextRib6 As String
        Dim TextRib7, TextRib8, TextRib9, TextRib10, TextRib11, TextRib12, TextRib13 As String
        Dim SelectedElement As String
    End Structure
    Friend WithEvents List01 As New System.Windows.Forms.RibbonButtonList
    Friend WithEvents List02 As New System.Windows.Forms.RibbonButtonList
    Dim A, B, C, D As Decimal 'equation Droite 3D AX+BY+CZ+D=0
    Public Force As GlobalStructure.ConcreteForce
    Public ProretyBeton As GlobalStructure.Comportement
    Structure AffichageNoeud
        Public Noued As Integer
        Public X, Y, Z, Force As Double
    End Structure
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles Me.Load
        SplitContainer1.Top = 0
        SplitContainer1.SplitterDistance = 0.8 * SplitContainer1.Height

        ProretyBeton.Kc = 0.6667
        ProretyBeton.PsiDegre = 5
        ProretyBeton.Excent = 0.1
        ProretyBeton.Rhou = 2500 ' kg/m3
        ProretyBeton.Comprission = New List(Of DoublePoint)
        ProretyBeton.Tension = New List(Of DoublePoint)

        ProretyBeton.Comprission.Add(New DoublePoint With {.y = 8.38, .x = 0})
        ProretyBeton.Comprission.Add(New DoublePoint With {.y = 9.98, .x = 0.000012})
        ProretyBeton.Comprission.Add(New DoublePoint With {.y = 11.15, .x = 0.000026})
        ProretyBeton.Comprission.Add(New DoublePoint With {.y = 12.81, .x = 0.000042})
        ProretyBeton.Comprission.Add(New DoublePoint With {.y = 13.92, .x = 0.000051})
        ProretyBeton.Comprission.Add(New DoublePoint With {.y = 15.71, .x = 0.000079})
        ProretyBeton.Comprission.Add(New DoublePoint With {.y = 17.56, .x = 0.000106})
        ProretyBeton.Comprission.Add(New DoublePoint With {.y = 20.03, .x = 0.00014})
        ProretyBeton.Comprission.Add(New DoublePoint With {.y = 22.12, .x = 0.00023})
        ProretyBeton.Comprission.Add(New DoublePoint With {.y = 23.67, .x = 0.00029})
        ProretyBeton.Comprission.Add(New DoublePoint With {.y = 25.21, .x = 0.00047})
        ProretyBeton.Comprission.Add(New DoublePoint With {.y = 26.07, .x = 0.00057})
        ProretyBeton.Comprission.Add(New DoublePoint With {.y = 27.4, .x = 0.00098})
        ProretyBeton.Comprission.Add(New DoublePoint With {.y = 26.88, .x = 0.0016})
        ProretyBeton.Comprission.Add(New DoublePoint With {.y = 24, .x = 0.0024})
        ProretyBeton.Comprission.Add(New DoublePoint With {.y = 20.31, .x = 0.0031})
        ProretyBeton.Comprission.Add(New DoublePoint With {.y = 15.88, .x = 0.0039})

        ProretyBeton.Tension.Add(New DoublePoint With {.y = 3.447, .x = 0})
        ProretyBeton.Tension.Add(New DoublePoint With {.y = 3.0, .x = 0.000025})
        ProretyBeton.Tension.Add(New DoublePoint With {.y = 2.5, .x = 0.000051})
        ProretyBeton.Tension.Add(New DoublePoint With {.y = 2.07, .x = 0.000075})
        ProretyBeton.Tension.Add(New DoublePoint With {.y = 1.72, .x = 0.000106})
        ProretyBeton.Tension.Add(New DoublePoint With {.y = 1.31, .x = 0.000164})
        ProretyBeton.Tension.Add(New DoublePoint With {.y = 1.04, .x = 0.000236})
        ProretyBeton.Tension.Add(New DoublePoint With {.y = 0.82, .x = 0.000316})
        ProretyBeton.Tension.Add(New DoublePoint With {.y = 0.63, .x = 0.000402})
        ProretyBeton.Tension.Add(New DoublePoint With {.y = 0.55, .x = 0.000484})

        ChargerTypes(List02, RibbonButton3)
        ChargerModeDessin(List01, RibbonButton7)
        OurProblem.CompressiveCase = True
        eyeX = 20
        eyeY = 20
        eyeZ = 10

        targetX = 0
        targetY = 0
        targetZ = 0.06
        DefinirEquationPlan()

        RibbonTextBox6.ToolTip = "Longueur (cm)"
        RibbonTextBox6.Enabled = True
        RibbonTextBox2.ToolTip = "Diamètre (cm)"
        RibbonTextBox1.ToolTip = "Hauteur (cm)"
        RibbonTextBox3.ToolTip = "Pour l'affichage"
        RibbonTextBox6.Text = "L(cm) = "
        RibbonTextBox2.Text = "D(cm) ="
        RibbonTextBox1.Text = "H(cm) ="
        RibbonTextBox3.TextBoxText = "30"

        Force.Type = 1 ' 0:fixe 1:increment
        Force.Valeur = -2
        ' Pour Ftm = 3.48 --> Fck = 39.195 et pour fcm = 27.4 ---> fck=19.4  E0=26000  fc0 = 10.96
        ProretyBeton.Fck = 22.5 ' 19.4
        ProretyBeton.E = 0
        ProretyBeton.V = 0.2
        ProretyBeton.Fb0_Fc0 = 1.16 '
        ProretyBeton.Approch = 1 ' 0: Alfarah  1:Bakhti   2:User Data
        Force.ChargeRepartie = True
    End Sub
    Private Sub DefinirEquationPlan()
        A = 0
        B = 0
        C = 0
        D = 0
        Dim A1, B1, C1 As Decimal
        Call EquationDroiteApartir2Points(eyeX, eyeY, targetX, targetY, A1, B1, C1)
        A = A1
        B = B1
        D = C1
        Call EquationDroiteApartir2Points(eyeX, eyeZ, targetX, targetZ, A1, B1, C1)
        A = A + A1
        C = B1
        D = D + C1
        Call EquationDroiteApartir2Points(eyeY, eyeZ, targetY, targetZ, A1, B1, C1)
        B = B + A1
        C = C + B1
        D = D + C1
    End Sub

    Private Sub List01_ButtonItemClicked(sender As Object, e As RibbonItemEventArgs) Handles List01.ButtonItemClicked
        RibbonButton7.Image = e.Item.Image
        RibbonButton7.SmallImage = e.Item.Image
        RibbonButton7.Value = e.Item.Value
        RibbonButton7.Text = e.Item.Text
        ModeDessin = e.Item.Text
        Redissigner(GlControl1)
        Redissigner(GlControl1)
    End Sub
    Private Sub List02_ButtonItemClicked(sender As Object, e As RibbonItemEventArgs) Handles List02.ButtonItemClicked
        RibbonButton3.Image = e.Item.Image
        RibbonButton3.SmallImage = e.Item.Image
        RibbonButton3.Value = e.Item.Value
        RibbonButton3.Text = e.Item.Text

        Select Case e.Item.Text
            Case "Cylindre"
                RibbonTextBox6.Text = "L(cm) ="
                RibbonTextBox6.Enabled = False
                RibbonTextBox2.Text = "D(cm) ="
                RibbonTextBox1.Text = "H(cm) ="

                RibbonTextBox6.ToolTip = "Longueur (cm)"
                RibbonTextBox2.ToolTip = "Diamètre (cm)"
                RibbonTextBox1.ToolTip = "Hauteur (cm)"

            Case "Cube"
                RibbonTextBox6.Text = "L(cm) ="
                RibbonTextBox6.Enabled = True
                RibbonTextBox2.Text = "L(cm) ="
                RibbonTextBox1.Text = "H(cm) ="

                RibbonTextBox6.ToolTip = "Longueur (cm)"
                RibbonTextBox2.ToolTip = "Largeur (cm)"
                RibbonTextBox1.ToolTip = "Hauteur (cm)"
        End Select

    End Sub
    Private Sub Config()
        Dim W2 As Integer = SplitContainer1.Panel2.Width
        Dim H2 As Integer = SplitContainer1.Panel2.Height
        Dim W1 As Integer = SplitContainer1.Panel1.Width
        Dim H1 As Integer = SplitContainer1.Panel1.Height

        ListBox1.Top = 0
        ListBox1.Left = 0
        ListBox1.Height = H2
        ListBox1.Width = W2
        GlControl1.Top = 0
        GlControl1.Left = 0
        GlControl1.Height = H1
        GlControl1.Width = W1
        ' Redissigner(GlControl1)
    End Sub
    Private Sub Form1_Resize(sender As Object, e As EventArgs) Handles Me.Resize

    End Sub

    Private Sub SplitContainer1_Resize(sender As Object, e As EventArgs) Handles SplitContainer1.Resize
        Config()
        Redissigner(GlControl1)
    End Sub

    Private Sub GlControl1_Load(sender As Object, e As EventArgs) Handles GlControl1.Load
        GL.ClearColor(Color.White)
        GL.Clear(ClearBufferMask.ColorBufferBit)
        GL.MatrixMode(MatrixMode.Projection)
        Redissigner(GlControl1)
    End Sub

    Private Sub SplitContainer1_SplitterMoved(sender As Object, e As SplitterEventArgs) Handles SplitContainer1.SplitterMoved
        Config()
        Redissigner(GlControl1)
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Me.WindowState = FormWindowState.Maximized
        Redissigner(GlControl1)
        Timer1.Enabled = False
    End Sub



    Private Sub RibbonButton4_Click(sender As Object, e As EventArgs) Handles RibbonButton4.Click
        CalculateMesh()
    End Sub
    Private Sub CalculateMesh()
        '  OurProblem.MaillgeParametre = 100
        OurProblem.MaillgeParametre = 20
        Dim L As Decimal = Decoval(RibbonTextBox6.TextBoxText) * 0.01
        Dim d As Decimal = Decoval(RibbonTextBox2.TextBoxText) * 0.01
        Dim H As Decimal = Decoval(RibbonTextBox1.TextBoxText) * 0.01
        OurProblem.GenerateMesh(RibbonButton3.Text, ProretyBeton.E, ProretyBeton.V, ProretyBeton.Fb0_Fc0,
                                  ProretyBeton.Kc, ProretyBeton.PsiDegre, ProretyBeton.Fck, ProretyBeton.Excent,
                                  ProretyBeton.Rhou, ProretyBeton.Comprission, ProretyBeton.Tension, d, H, L)
        Redissigner(GlControl1)
    End Sub

    Private Sub RibbonButton5_Click(sender As Object, e As EventArgs) Handles RibbonButton5.Click
        BroadenMesh()
    End Sub
    Private Sub BroadenMesh()
        If OurProblem.Noeuds.Count = 0 Then
            MsgBox("IL faut faire le Maillage")
            Exit Sub
        End If
        OurProblem.MaillgeParametre = 1.2 * OurProblem.MaillgeParametre
        '    OurProblem.MaillgeParametre = 60
        OurProblem.GenerateMesh(RibbonButton3.Text, ProretyBeton.E, ProretyBeton.V, ProretyBeton.Fb0_Fc0,
                                  ProretyBeton.Kc, ProretyBeton.PsiDegre, ProretyBeton.Fck, ProretyBeton.Excent, ProretyBeton.Rhou,
                                  ProretyBeton.Comprission, ProretyBeton.Tension,
                                  Decoval(RibbonTextBox2.TextBoxText) * 0.01, Decoval(RibbonTextBox1.TextBoxText) * 0.01,
                                  Decoval(RibbonTextBox6.TextBoxText) * 0.01)
        Redissigner(GlControl1)
    End Sub

    Private Sub RibbonButton6_Click(sender As Object, e As EventArgs) Handles RibbonButton6.Click
        SqueezeMesh()
    End Sub
    Private Sub SqueezeMesh()
        If OurProblem.Noeuds.Count = 0 Then
            MsgBox("IL faut faire le Maillage")
            Exit Sub
        End If
        OurProblem.MaillgeParametre = 0.8 * OurProblem.MaillgeParametre
        OurProblem.GenerateMesh(RibbonButton3.Text, ProretyBeton.E, ProretyBeton.V, ProretyBeton.Fb0_Fc0,
                                  ProretyBeton.Kc, ProretyBeton.PsiDegre, ProretyBeton.Fck, ProretyBeton.Excent, ProretyBeton.Rhou,
                                  ProretyBeton.Comprission, ProretyBeton.Tension,
                                  Decoval(RibbonTextBox2.TextBoxText) * 0.01,
                                  Decoval(RibbonTextBox1.TextBoxText) * 0.01, Decoval(RibbonTextBox6.TextBoxText) * 0.01)
        Redissigner(GlControl1)
    End Sub
    Private Sub GlControl1_MouseWheel(sender As Object, e As MouseEventArgs) Handles GlControl1.MouseWheel
        If ModeDessin = "3D" Then
            If e.Delta > 0 Then
                If Abs(targetX - eyeX) > 0.1 And Abs(targetY - eyeY) > 0.1 Then
                    eyeX = eyeX + e.Delta * (targetX - eyeX) / 1000
                    eyeY = eyeY + e.Delta * (targetY - eyeY) / 1000
                    eyeZ = A * eyeX + B * eyeY + D
                    eyeZ = eyeZ / (-C)
                End If
            Else
                eyeX = eyeX + e.Delta * (targetX - eyeX) / 1000
                eyeY = eyeY + e.Delta * (targetY - eyeY) / 1000
                eyeZ = A * eyeX + B * eyeY + D
                eyeZ = eyeZ / (-C)
            End If
        End If
        Redissigner(GlControl1)
    End Sub

    Private Sub RibbonButton8_Click(sender As Object, e As EventArgs) Handles RibbonButton8.Click
        CalculateLinear()
    End Sub
    Private Sub CalculateLinear()
        ProretyBeton.E = Decoval(RibbonTextBox4.TextBoxText)
        ProretyBeton.V = Decoval(RibbonTextBox13.TextBoxText)
        Force.Type = 1 ' 0:fixe 1:increment
        Force.Valeur = Decoval(RibbonTextBox5.TextBoxText)
        ProretyBeton.Comprission = New List(Of Base.DoublePoint)
        ProretyBeton.Tension = New List(Of Base.DoublePoint)
        ChargerProprite(ProretyBeton)
        LancerCalcul(0)
        Redissigner(GlControl1)
    End Sub
    Private Sub LancerCalcul(Cas As Integer)

        If OurProblem.Noeuds.Count = 0 Then
            MsgBox("IL faut faire le Maillage")
            Exit Sub
        End If
        OurProblem.Force = Force
        OurProblem.PropretyBeton = ProretyBeton

        OurProblem.ResoudreProbleme(Cas)
        ListBox1.Items.Clear()

        Affichage(Cas, True)
    End Sub
    Private Sub Affichage(cas As Integer, DeplacementMax As Boolean)
        Try
            ListBox1.Items.Clear()
            Select Case cas
                Case 0
                    ListBox1.Items.Add("Mode: Linear")
                    ListBox1.Items.Add("-------------------------------------------------------------------------------------------------------------")
                Case 1
                    ListBox1.Items.Add("Mode: Non-Linear, Model: Plastic Damage Model")
                    ListBox1.Items.Add("-------------------------------------------------------------------------------------------------------------")
                Case 2
                    ListBox1.Items.Add("Mode: Non-Linear, Model: Plastic Damage Model ")
                    ListBox1.Items.Add("-------------------------------------------------------------------------------------------------------------")
            End Select
            ListBox1.Items.Add("Displacement (m)")
            Dim jp, Noued As Decimal
            Dim Resultats() As AffichageNoeud
            For i = 0 To OurProblem.IndexNoued.Count - 1
                Dim Nbr As Integer
                Noued = Int(i / 3)

                jp = i - 3 * Int(i / 3)
                Dim II As Integer = OurProblem.IndexNoued.Item(i)
                If II >= 0 Then
                    Dim ExistNoeud As Boolean = False
                    If Not IsNothing(Resultats) Then
                        For j = 0 To Resultats.Count - 1
                            If Resultats(j).Noued = Noued Then
                                ExistNoeud = True
                                Nbr = j
                            End If
                        Next
                    End If
                    If ExistNoeud = False Then
                        If IsNothing(Resultats) Then
                            Nbr = 0
                        Else
                            Nbr = Resultats.Count
                        End If
                        ReDim Preserve Resultats(Nbr)
                        Resultats(Nbr).Noued = Noued
                    End If
                    Select Case jp
                        Case 0
                            Resultats(Nbr).X = OurProblem.Deplacement(II)
                        Case 1
                            Resultats(Nbr).Y = OurProblem.Deplacement(II)
                        Case 2
                            Resultats(Nbr).Z = OurProblem.Deplacement(II)
                    End Select
                    Resultats(Nbr).Force = OurProblem.Forces(II)
                End If
            Next
            Dim AffResultats As List(Of AffichageNoeud) = ConfigAffichage(Resultats, DeplacementMax)
            If Not IsNothing(AffResultats) Then
                For i = 0 To AffResultats.Count - 1
                    ListBox1.Items.Add("Node N° " & (AffResultats.Item(i).Noued + 1).ToString & "  Z= " & AffResultats.Item(i).Z & "  X= " & AffResultats.Item(i).X & "  Y= " & AffResultats.Item(i).Y &
                                         "-->Load= " & AffResultats.Item(i).Force)
                Next
            End If
        Catch
            ListBox1.Items.Clear()
        End Try
    End Sub
    Private Function ConfigAffichage(Donnees() As AffichageNoeud, DeplacementMax As Boolean) As List(Of AffichageNoeud)
        Dim Resultats As New List(Of AffichageNoeud)
        If Donnees.Count > 0 Then

            Dim Donnee As List(Of AffichageNoeud) = Donnees.ToList
            If DeplacementMax Then
                Do
                    Dim Count As Integer = 0
                    Dim Zmax As Double
                    Zmax = -10 ^ 20
                    For i = 0 To Donnee.Count - 1
                        If Donnee.Item(i).Z > Zmax Then
                            Zmax = Donnee.Item(i).Z
                            Count = i
                        End If
                    Next
                    Resultats.Add(Donnee.Item(Count))
                    Donnee.Remove(Donnee.Item(Count))
                Loop While Donnee.Count > 0
                Return Resultats
            Else
                Return Donnee
            End If

        Else
            Return Nothing
        End If
    End Function

    Private Sub RibbonButton12_Click(sender As Object, e As EventArgs) Handles RibbonButton12.Click
        CalculateDPM()
    End Sub
    Private Sub CalculateDPM()
        'ProretyBeton.E = Decoval(TextBox1.Text)
        ProretyBeton.V = Decoval(RibbonTextBox12.TextBoxText)
        ProretyBeton.Approch = 1 'bakhti
        ProretyBeton.Comprission = New List(Of Base.DoublePoint)
        ProretyBeton.Tension = New List(Of Base.DoublePoint)
        ProretyBeton.Fb0_Fc0 = Decoval(RibbonTextBox7.TextBoxText)
        ProretyBeton.PsiDegre = Decoval(RibbonTextBox8.TextBoxText)
        ProretyBeton.Excent = Decoval(RibbonTextBox9.TextBoxText)
        ProretyBeton.Kc = Decoval(RibbonTextBox10.TextBoxText)
        ProretyBeton.Fck = Decoval(RibbonTextBox11.TextBoxText) - 8
        ChargerProprite(ProretyBeton)
        LancerCalcul(2)
        Redissigner(GlControl1)
    End Sub

    Private Sub RibbonTextBox3_TextBoxTextChanged(sender As Object, e As EventArgs) Handles RibbonTextBox3.TextBoxTextChanged
        EchelleAffichage = Decoval(RibbonTextBox3.TextBoxText)
        Redissigner(GlControl1)
    End Sub

    Private Sub RibbonButton13_Click(sender As Object, e As EventArgs) Handles RibbonButton13.Click
        Curve.ShowDialog()
    End Sub

    Private Sub CloseButton_Click(sender As Object, e As EventArgs) Handles CloseButton.Click
        End
    End Sub

    Private Sub CalculateLinearItem_Click(sender As Object, e As EventArgs) Handles CalculateLinearItem.Click
        CalculateLinear()
    End Sub

    Private Sub CalculateDPMItem_Click(sender As Object, e As EventArgs) Handles CalculateDPMItem.Click
        CalculateDPM()
    End Sub

    Private Sub ResultsItem_Click(sender As Object, e As EventArgs) Handles ResultsItem.Click
        Curve.ShowDialog()
    End Sub

    Private Sub CalculateMeshItem_Click(sender As Object, e As EventArgs) Handles CalculateMeshItem.Click
        CalculateMesh()
    End Sub

    Private Sub BroadenItem_Click(sender As Object, e As EventArgs) Handles BroadenItem.Click
        BroadenMesh()
    End Sub

    Private Sub SqueezeItem_Click(sender As Object, e As EventArgs) Handles SqueezeItem.Click
        SqueezeMesh()
    End Sub

    Private Sub SaveProjectItem_Click(sender As Object, e As EventArgs) Handles SaveProjectItem.Click
        Dim FenetreDialog As New SaveFileDialog
        Dim MyPath As String
        FenetreDialog.Filter = "Concrete Workbook|*.conc"
        FenetreDialog.Title = "Save Concrete File"
        Dim Dialog As DialogResult = FenetreDialog.ShowDialog()
        If Dialog = DialogResult.OK Then
            MyPath = FenetreDialog.FileName
        Else
            Exit Sub
        End If


        Dim ConcreteData As ConcreteData
        Dim FileNo As Integer = FreeFile()
        FileOpen(FileNo, MyPath, OpenMode.Binary)
        '-----------------------------  Save index


        ConcreteData.TextRib1 = RibbonTextBox1.TextBoxText
        ConcreteData.TextRib2 = RibbonTextBox2.TextBoxText
        ConcreteData.TextRib3 = RibbonTextBox3.TextBoxText
        ConcreteData.TextRib4 = RibbonTextBox4.TextBoxText
        ConcreteData.TextRib5 = RibbonTextBox5.TextBoxText
        ConcreteData.TextRib6 = RibbonTextBox6.TextBoxText
        ConcreteData.TextRib7 = RibbonTextBox7.TextBoxText
        ConcreteData.TextRib8 = RibbonTextBox8.TextBoxText
        ConcreteData.TextRib9 = RibbonTextBox9.TextBoxText
        ConcreteData.TextRib10 = RibbonTextBox10.TextBoxText
        ConcreteData.TextRib11 = RibbonTextBox11.TextBoxText
        ConcreteData.TextRib12 = RibbonTextBox12.TextBoxText
        ConcreteData.TextRib13 = RibbonTextBox13.TextBoxText
        ConcreteData.SelectedElement = RibbonButton3.Value


        FilePut(FileNo, ConcreteData)

        FileClose(FileNo)
        FenetreDialog.Dispose()
        MsgBox("The project is saved", vbInformation, "Concrete v2.0.0")

    End Sub

    Private Sub OpenProjectItem_Click(sender As Object, e As EventArgs) Handles OpenProjectItem.Click
        Dim FenetreDialog As New OpenFileDialog With {.Filter = "Concrete Workbook|*.conc", .Title = "Open Concrete File"}
        Dim Dialog As DialogResult = FenetreDialog.ShowDialog()
        If Dialog <> DialogResult.OK Then
            FenetreDialog.Dispose()
            Exit Sub
        End If

        Dim MyPath As String = FenetreDialog.FileName
        Dim FileNo As Integer = FreeFile()
        Dim ConcreteData As ConcreteData
        Try

            FileOpen(FileNo, MyPath, OpenMode.Binary)

            FileGet(FileNo, ConcreteData)


            RibbonTextBox1.TextBoxText = ConcreteData.TextRib1
            RibbonTextBox2.TextBoxText = ConcreteData.TextRib2
            RibbonTextBox3.TextBoxText = ConcreteData.TextRib3
            RibbonTextBox4.TextBoxText = ConcreteData.TextRib4
            RibbonTextBox5.TextBoxText = ConcreteData.TextRib5
            RibbonTextBox6.TextBoxText = ConcreteData.TextRib6
            RibbonTextBox7.TextBoxText = ConcreteData.TextRib7
            RibbonTextBox8.TextBoxText = ConcreteData.TextRib8
            RibbonTextBox9.TextBoxText = ConcreteData.TextRib9
            RibbonTextBox10.TextBoxText = ConcreteData.TextRib10
            RibbonTextBox11.TextBoxText = ConcreteData.TextRib11
            RibbonTextBox12.TextBoxText = ConcreteData.TextRib12
            RibbonTextBox13.TextBoxText = ConcreteData.TextRib13

            If ConcreteData.SelectedElement = "0" Then
                RibbonButton3.Image = List02.Buttons.Item(0).Image
                RibbonButton3.SmallImage = List02.Buttons.Item(0).Image
                RibbonButton3.Text = "Cylindre"
                RibbonTextBox6.Enabled = False
                RibbonButton3.Value = 1
            ElseIf ConcreteData.SelectedElement = "1" Then
                RibbonButton3.Image = List02.Buttons.Item(1).Image
                RibbonButton3.SmallImage = List02.Buttons.Item(1).Image
                RibbonButton3.Text = "Cube"
                RibbonButton3.Value = 1
                RibbonTextBox6.Enabled = True
            End If


            FileClose(FileNo)
            FenetreDialog.Dispose()
            MsgBox("The project is open", vbInformation, "Concrete v2.0.0")
        Catch
            MsgBox("Error: cannot open the project", vbCritical, "Concrete v2.0.0")
        End Try
    End Sub

    Private Sub NewProjectItem_Click(sender As Object, e As EventArgs) Handles NewProjectItem.Click
        OurProblem = Nothing
        OurProblem = New GlobalStructure
        Redissigner(GlControl1)
        ListBox1.Items.Clear()
    End Sub

    Private Sub RibbonButton9_Click(sender As Object, e As EventArgs) Handles RibbonButton9.Click
        If OurProblem.CompressiveCase Then
            OurProblem.CompressiveCase = False
            Me.RibbonButton9.Image = Global.Doctorat.My.Resources.Resources.switchOff
            Me.RibbonButton9.LargeImage = Global.Doctorat.My.Resources.Resources.switchOff
            Me.RibbonButton9.Text = "Tension"
        Else
            OurProblem.CompressiveCase = True
            Me.RibbonButton9.Image = Global.Doctorat.My.Resources.Resources.switchOn
            Me.RibbonButton9.LargeImage = Global.Doctorat.My.Resources.Resources.switchOn
            Me.RibbonButton9.Text = "Compression"

        End If
    End Sub

    Private Sub RibbonButton1_Click(sender As Object, e As EventArgs) Handles RibbonButton1.Click
        MsgBox("Option is not available in this version", vbInformation, "Concrete v2.0.0")
    End Sub

    Private Sub RibbonButton2_Click(sender As Object, e As EventArgs) Handles RibbonButton2.Click
        MsgBox("Option is not available in this version", vbInformation, "Concrete v2.0.0")
    End Sub
End Class

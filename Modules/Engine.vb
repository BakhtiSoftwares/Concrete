Imports OpenTK
Imports OpenTK.Platform
Imports OpenTK.Graphics.OpenGL
Imports System.IO
Imports Doctorat.Base
Module Engine
    Public OurProblem As New GlobalStructure
    Public ModeDessin As String
    Public eyeX, eyeY, eyeZ, targetX, targetY, targetZ As Double
    Public EchelleAffichage As Integer
    Public Sub ChargerProprite(ProretyBeton As GlobalStructure.Comportement)
        OurProblem.PropretyBeton = ProretyBeton
        For Each Elem In OurProblem.Elements
            Elem.Material.V = ProretyBeton.V
            Elem.Comprission = ProretyBeton.Comprission
            Elem.Tension = ProretyBeton.Tension
            Elem.Material.Fb0_Fc0 = ProretyBeton.Fb0_Fc0
            Elem.Material.PsiDegre = ProretyBeton.PsiDegre
            Elem.Material.Excent = ProretyBeton.Excent
            Elem.Material.Kc = ProretyBeton.Kc
            Elem.Material.Fck = ProretyBeton.Fck
            Elem.Material.E0 = ProretyBeton.E
        Next
    End Sub
    Public Function IndexDraw(Text As String) As Integer
        Select Case Text

            Case "Loads-Displacement"
                Return 0
            Case "Compressive: Stress-Strain"
                Return 1
            Case "Damage-Parameter Curve"
                Return 2
            Case "Tensile: Stress-Strain"
                Return 3
            Case "Compressive CDP Model Curve: Bakhti"
                Return 4
            Case "Compressive Damage Parameter Curve: Bakhti"
                Return 5
            Case "Tensile CDP Model Curve: Bakhti"
                Return 6
            Case "Tensile CDP Damage Parameter Curve: Bakhti"
                Return 7
            Case "Compressive CDP Model Curve: Alfarah"
                Return 8
            Case "Compressive Damage Parameter Curve: Alfarah"
                Return 9
            Case "Tensile CDP Model Curve: Alfarah"
                Return 10
            Case "Tensile Damage Parameter Curve: Alfarah"
                Return 11
            Case "Compressive Damage-Parameter Curve"
                Return 12
        End Select
        Return 1
    End Function
    Public Sub SaveBitmap(Bitmap As Bitmap)
        Dim FenetreDialog As New SaveFileDialog
        FenetreDialog.Filter = "Bitmap|*.bmp"
        FenetreDialog.Title = "Save Bitmap File"
        Dim Dialog As DialogResult = FenetreDialog.ShowDialog()
        If Dialog = DialogResult.OK Then
            Bitmap.Save(FenetreDialog.FileName)
            MsgBox("Opération effectuée", vbInformation, "CiviLab")
        End If
    End Sub
    Public Sub SaveBitmap(W As Integer, H As Integer, SelectedIndex As Integer, WidthPen As Integer)
        Dim Bitmap As New Bitmap(W, H)
        Dim E As Drawing.Graphics = Drawing.Graphics.FromImage(Bitmap)
        E.FillRectangle(New SolidBrush(Color.White), New Rectangle With {.Height = H, .Width = W,
               .Location = New Point With {.X = 0, .Y = 0}})
        DrawingGraph(W, H, SelectedIndex, E, WidthPen)
        SaveBitmap(Bitmap)
    End Sub
    Public Sub DrawingGraph(W As Integer, H As Integer, SelectedIndex As Integer, ByRef E As Drawing.Graphics, WidthPen As Integer)
        E.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
        E.TextRenderingHint = Drawing.Text.TextRenderingHint.AntiAliasGridFit
        E.InterpolationMode = Drawing2D.InterpolationMode.High

        Select Case SelectedIndex
            Case 0
                OurProblem.DrawLoadDisplacementCurve(E, W, H, Curve.NumericUpDown1.Value, Curve.NumericUpDown2.Value, Curve.NumericUpDown3.Value, WidthPen)

            Case 1
                OurProblem.DrawComprissiveStressStrainCurve(E, W, H, Curve.NumericUpDown1.Value, Curve.NumericUpDown2.Value, Curve.NumericUpDown3.Value, WidthPen)

            Case 2
                OurProblem.DrawDamageParameterCurve(E, W, H, Curve.NumericUpDown1.Value, Curve.NumericUpDown2.Value, Curve.NumericUpDown3.Value, WidthPen)

            Case 3
                OurProblem.DrawTensileStressStrainCurve(E, W, H, Curve.NumericUpDown1.Value, Curve.NumericUpDown2.Value, Curve.NumericUpDown3.Value, WidthPen)

            Case 4
                OurProblem.DrawComprissiveCDPMCurveBakhti(E, W, H, Curve.NumericUpDown1.Value, Curve.NumericUpDown2.Value, Curve.NumericUpDown3.Value, WidthPen)

            Case 5
                OurProblem.DrawComprissiveDamageParameterBakhti(E, W, H, Curve.NumericUpDown1.Value, Curve.NumericUpDown2.Value, Curve.NumericUpDown3.Value, WidthPen)

            Case 6
                OurProblem.DrawTensileCDPMCurveBakhti(E, W, H, Curve.NumericUpDown1.Value, Curve.NumericUpDown2.Value, Curve.NumericUpDown3.Value, WidthPen)

            Case 7
                OurProblem.DrawTensileDamageParameterBakhti(E, W, H, Curve.NumericUpDown1.Value, Curve.NumericUpDown2.Value, Curve.NumericUpDown3.Value, WidthPen)

            Case 8
                OurProblem.DrawComprissiveCDPMCurveAlfarah(E, W, H, Curve.NumericUpDown1.Value, Curve.NumericUpDown2.Value, Curve.NumericUpDown3.Value, WidthPen)

            Case 9
                OurProblem.DrawComprissiveDamageParameterAlfarah(E, W, H, Curve.NumericUpDown1.Value, Curve.NumericUpDown2.Value, Curve.NumericUpDown3.Value, WidthPen)

            Case 10
                OurProblem.DrawTensileCDPMCurveAlfarah(E, W, H, Curve.NumericUpDown1.Value, Curve.NumericUpDown2.Value, Curve.NumericUpDown3.Value, WidthPen)

            Case 11
                OurProblem.DrawTensileDamageParameterAlfarah(E, W, H, Curve.NumericUpDown1.Value, Curve.NumericUpDown2.Value, Curve.NumericUpDown3.Value, WidthPen)

            Case 12
                OurProblem.DrawComprissiveDamageParameterCurve(E, W, H, Curve.NumericUpDown1.Value, Curve.NumericUpDown2.Value, Curve.NumericUpDown3.Value, WidthPen)
        End Select
    End Sub
    Public Sub ChargerGLControl(MyGlControl As GLControl)

        GL.ClearColor(Color.White)
        GL.Clear(ClearBufferMask.ColorBufferBit)
        GL.MatrixMode(MatrixMode.Projection)
        GL.LoadIdentity()

        GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line)
        GL.Clear(ClearBufferMask.ColorBufferBit)
        GL.ClearColor(Color.Black)
        GL.MatrixMode(MatrixMode.Projection)
        GL.LoadIdentity()
        MyGlControl.SwapBuffers()
    End Sub
    Public Sub ReDraw2D(MyGlControl As GLControl)
        Dim w, h As Integer
        w = MyGlControl.Width
        h = MyGlControl.Height
        Dim Rapport As Double = w / h
        GL.ClearColor(Color.Black)
        GL.Clear(ClearBufferMask.ColorBufferBit)
        GL.LoadIdentity()
        GL.MatrixMode(MatrixMode.Projection)
        ' GL.Ortho(-0.1 * Rapport, 0.1 * Rapport, -0.1, 0.1, -1, 1)
        GL.Ortho(-1, 1, -1, 1, -1, 1)
        GL.Viewport(0, 0, w, h)
        OurProblem.Draw2D(Color.Red)

    End Sub
    Public Sub ReDraw3D(MyGlControl As GLControl)
        Dim w, h As Integer
        w = MyGlControl.Width
        h = MyGlControl.Height
        Dim Rapport As Double = w / h
        Dim Prespective As Matrix4 = Matrix4.CreatePerspectiveFieldOfView(1, Rapport, 0.01, 100)
        Dim LoackAt As Matrix4 = Matrix4.LookAt(eyeX, eyeY, eyeZ, targetX, targetY, targetZ, 0, 0, 1)
        GL.MatrixMode(MatrixMode.Projection)
        GL.LoadIdentity()
        GL.LoadMatrix(Prespective)
        GL.MatrixMode(MatrixMode.Modelview)
        GL.LoadIdentity()
        GL.LoadMatrix(LoackAt)
        GL.Viewport(0, 0, w, h)

        GL.ClearColor(Color.Black)
        GL.Clear(ClearBufferMask.ColorBufferBit)
        OurProblem.Draw3D(True, Color.Red)
        OurProblem.Draw3D(False, Color.SkyBlue, EchelleAffichage)
    End Sub
    Public Function DetM(A11 As Double, A12 As Double, A21 As Double, A22 As Double) As Double
        Return A11 * A22 - A12 * A21
    End Function

    Public Sub Redissigner(MyGlControl As GLControl)
        If ModeDessin = "2D" Then
            ReDraw2D(MyGlControl)
        End If
        If ModeDessin = "3D" Then
            ReDraw3D(MyGlControl)
        End If
        MyGlControl.SwapBuffers()
    End Sub
    Private Function ListOfPng(Path As String) As List(Of Bitmap)
        On Error GoTo 100
        Dim dir1 As New DirectoryInfo(Path)
        Dim ObjectFile As FileInfo() = dir1.GetFiles("*.png")
        Dim FileNo As Integer
        Dim ObjectNumbres As Integer = ObjectFile.Count
        Dim Pourcentage As Integer
        ListOfPng = New List(Of Bitmap)
        For f = 0 To ObjectFile.Count - 1
            Dim MyPng As New Bitmap(ObjectFile(f).FullName)
            ListOfPng.Add(MyPng)
        Next
100:
    End Function
    Public Sub ChargerModeDessin(list As RibbonButtonList, RibbonButton As RibbonButton)

        Dim ListOFPicture As List(Of Bitmap) = ListOfPng(Application.StartupPath & "\Pic\2") ' \Arbres
        Dim k As Integer
        k = ListOFPicture.Count - 1

        list.ItemsSizeInDropwDownMode = New System.Drawing.Size(1, 2)
        Dim size As New Size With {.Height = 20, .Width = 50}
        For i = 0 To k
            Dim bt2 As New RibbonButton(RibbonElementSizeMode.Large)
            bt2.Image = ListOFPicture.Item(i)
            bt2.Value = i
            bt2.MinimumSize = size
            If i = 0 Then bt2.Text = "2D"
            If i = 1 Then bt2.Text = "3D"
            list.Buttons.Add(bt2)
        Next
        RibbonButton.DropDownItems.Add(list)
        RibbonButton.Image = ListOFPicture.Item(0)
        RibbonButton.SmallImage = ListOFPicture.Item(0)
        RibbonButton.Text = "2D"
        RibbonButton.Value = 0
        ModeDessin = "2D"
    End Sub
    Public Sub ChargerTypes(list As RibbonButtonList, RibbonButton As RibbonButton)

        Dim ListOFPicture As List(Of Bitmap) = ListOfPng(Application.StartupPath & "\Pic\1") ' \Arbres
        Dim k As Integer
        k = ListOFPicture.Count - 1

        list.ItemsSizeInDropwDownMode = New System.Drawing.Size(1, 2)
        Dim size As New Size With {.Height = 20, .Width = 50}
        For i = 0 To k
            Dim bt2 As New RibbonButton(RibbonElementSizeMode.Large)
            bt2.Image = ListOFPicture.Item(i)
            bt2.Value = i
            bt2.MinimumSize = size
            If i = 0 Then bt2.Text = "Cylindre"
            If i = 1 Then bt2.Text = "Cube"
            list.Buttons.Add(bt2)
        Next
        RibbonButton.DropDownItems.Add(list)
        RibbonButton.Image = ListOFPicture.Item(1)
        RibbonButton.SmallImage = ListOFPicture.Item(1)
        RibbonButton.Text = "Cube"
        RibbonButton.Value = 1
    End Sub
    Public Function Decoval(vale As String) As Double
        On Error GoTo 100
        Decoval = CDbl(vale)
        Exit Function
100:
        Decoval = Val(vale)
    End Function
End Module

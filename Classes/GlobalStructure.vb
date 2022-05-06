Imports TriangleNet
Imports TriangleNet.Geometry
Imports TriangleNet.Meshing
Imports TriangleNet.Playground
Imports TriangleNet.Rendering
Imports OpenTK.Graphics.OpenGL
Imports MathNet
Imports System.Math
Imports System.Drawing
Imports OOFEMcli.Structure
Public Class GlobalStructure
    Inherits Base

    Structure Quadralateral
        Public S1 As Integer
        Public S2 As Integer
        Public S3 As Integer
        Public S4 As Integer
    End Structure

    Structure Segment
        Public P1 As DoublePoint
        Public P2 As DoublePoint
    End Structure
    Public Structure ConcreteForce
        Public Type As Integer ' 0: fixe 1: increment
        Public Valeur As Double
        Public ChargeRepartie As Boolean
    End Structure
    Public Structure Comportement
        Public E, V, Fb0_Fc0, Kc, PsiDegre, Fck, Excent, Rhou As Double
        Public Approch As Integer
        Public Comprission As List(Of DoublePoint)
        Public Tension As List(Of DoublePoint)
    End Structure
    Public ListOfVertex As New List(Of DoublePoint)
    Public QuadMesh As New List(Of Quadralateral)
    Public Noeuds As New List(Of Node)
    Public Elements As New List(Of BrickEightNodes)
    Public MaillgeParametre As Double
    Public TypeElement As String
    Dim samp(4, 2), KGlob(,) As Double
    Public IndexNoued As New List(Of Integer)
    Public Deplacement(), Forces() As Double
    Public CompressiveCase As Boolean
    Dim MaxLong, Diametre, Haut, SurfaceTotal, Longeur As Double
    Dim Nq, NbrNoeudEtage As Integer
    Public Force As ConcreteForce
    Public PropretyBeton As Comportement
    Public NoeudProcheCentre As DoublePoint
    Dim Lmin As Double 'Longeur Min dans tous les elements
    Const Ngp = 2 'Nombre de points de gausse


    Public Sub GenerateMesh(_TypeElement As String, E As Double, V As Double, Fb0_Fc0 As Double, Kc As Double, PsiDegre As Double,
            Fck As Double, Excent As Double, Rhou As Double, Comprission As List(Of DoublePoint), Tension As List(Of DoublePoint),
                            Optional _Diametre As Double = 0, Optional _Haut As Double = 0, Optional Lon As Double = 0)
        TypeElement = _TypeElement
        Select Case TypeElement
            Case "Cylindre"
                Diametre = _Diametre
                Haut = _Haut
                GenerateMeshCylindre(Diametre, Haut, E, V, Fb0_Fc0, Kc, PsiDegre, Fck, Excent, Rhou, Comprission, Tension)
            Case "Cube"
                Diametre = _Diametre
                Haut = _Haut
                Longeur = Lon
                GenerateMeshCube(Haut, Diametre, Lon, E, V, Fb0_Fc0, Kc, PsiDegre, Fck, Excent, Rhou, Comprission, Tension) 'Diametre = largeur
        End Select
    End Sub


    Private Sub Conditionlimite()
        Dim lh As Integer = 3 * NbrNoeudEtage - 1

        For i As Integer = 0 To lh
            Call Condlim(i)
        Next

    End Sub
    Private Sub Condlim(ligne As Integer)
        If IndexNoued.Item(ligne) = -1 Then
            Exit Sub
        End If
        IndexNoued.Item(ligne) = -1
        For i = ligne + 1 To 3 * Noeuds.Count - 1
            If IndexNoued.Item(i) <> -1 Then IndexNoued.Item(i) = IndexNoued.Item(i) - 1
        Next
        Nq -= 1

    End Sub
    Public Sub ResoudreProbleme(Type As Integer)
        'Type ---> 0: léniaire , 1: Conctrete damaged plasticity mode viscoplastic (Initial strain)  , 2: Conctrete damaged plasticity mode nitial stress 

        Select Case Type
            Case 0
                ResoudreProblemeLineaire()
            Case 2
                ResoudreProblemeNonLineaireInitialStress()
        End Select

    End Sub
    Public Sub ResoudreProblemeLineaire()
        Dim i As Integer
        Nq = 3 * Noeuds.Count - 1
        ' les conditions limites
        IndexNoued.Clear()

        For i = 0 To Nq
            Dim Ind As Integer = i
            IndexNoued.Add(Ind)
        Next

        Call Conditionlimite()
        '    Force(2) = Val(Text7.Text)
        ReDim Deplacement(Nq)
        ReDim Forces(Nq)
        ReDim KGlob(Nq, Nq)
        Call Gauss(Ngp)
        Dim FrontElement As Boolean

        SurfaceTotal = 0
        Dim Cont As Integer
        For i = 0 To Elements.Count - 1
            Elements.Item(i).InitiStress(Ngp)
            Elements.Item(i).InitiStrain(Ngp)
            If i <= Elements.Count - 1 And i > Elements.Count - QuadMesh.Count - 1 Then
                SurfaceTotal += Surface(i)
                Cont += 1
            End If
        Next


        For i = 0 To Elements.Count - 1
            ' FORMULATION DE LA MATRICE DE RIGIDITE 
            Dim Ke(,) As Double = Elements.Item(i).MtriceRigiditeElementaire(Ngp, IndexNoued, samp)
            ' Assemblage de la matrice de rigidité

            Dim jp, jq, NI, NJ As Single
            Dim ii, jj As Integer
            For ip = 1 To 24
                For iq = 1 To 24
                    jp = ip - 3 * Int(ip / 3)
                    jq = iq - 3 * Int(iq / 3)
                    If jp = 0 Then jp = 3
                    If jq = 0 Then jq = 3
                    NI = Int(ip / 3)
                    If NI <> ip / 3 Then NI += 1
                    NJ = Int(iq / 3)
                    If NJ <> iq / 3 Then NJ += 1
                    ii = 3 * Elements.Item(i).ListNoeud(NI - 1).Ident + jp - 1
                    jj = 3 * Elements.Item(i).ListNoeud(NJ - 1).Ident + jq - 1
                    ii = IndexNoued.Item(ii)
                    jj = IndexNoued.Item(jj)
                    If ii <> -1 And jj <> -1 Then
                        KGlob(ii, jj) += Elements.Item(i).Ke(ip, iq)
                    End If
                Next
            Next

            ' FORMULATION DE VECTEUR DES FORCES
            FrontElement = False
            If i <= Elements.Count - 1 And i > Elements.Count - QuadMesh.Count - 1 Then FrontElement = True
            If FrontElement Then
                Dim Fe() As Double = VecteurForceElementaire(i, Force, Force.Valeur * SurfaceTotal, FrontElement)
                ' Assemblage de vecteur des forces
                For ip = 1 To 24
                    jp = ip - 3 * Int(ip / 3)
                    If jp = 0 Then jp = 3
                    NI = Int(ip / 3)
                    If NI <> ip / 3 Then NI += 1
                    ii = 3 * Elements.Item(i).ListNoeud(NI - 1).Ident + jp - 1
                    ii = IndexNoued.Item(ii)
                    If ii <> -1 Then Forces(ii) += Fe(ip)
                Next
            End If
        Next


        Call TrouverDeplacements()
        Call TrouverContraintes(Force.Valeur)
        MsgBox("The calculation is finished")
    End Sub

    Public Sub ResoudreProblemeNonLineaireInitialStrain()
        If PropretyBeton.E = 0 Then PropretyBeton.E = ModuleElasticite(PropretyBeton.Fck)
        Dim DT As Double
        '  DT = StepTime(True, PropretyBeton.PsiDegre, PropretyBeton.V, PropretyBeton.E * 10 ^ 6, Lmin, PropretyBeton.Rhou)
        DT = StepTime(False, PropretyBeton.PsiDegre, PropretyBeton.V, PropretyBeton.E * 10 ^ 6, Lmin, PropretyBeton.Rhou)



        Dim i As Integer
        Nq = 3 * Noeuds.Count - 1
        ' les conditions limites
        Dim MaxIteration As Integer = 1000
        IndexNoued.Clear()

        For i = 0 To Nq
            Dim Ind As Integer = i
            IndexNoued.Add(Ind)
        Next

        SurfaceTotal = 0
        For i = 0 To Elements.Count - 1
            If i <= Elements.Count - 1 And i > Elements.Count - QuadMesh.Count - 1 Then
                SurfaceTotal += Surface(i)
            End If
        Next

        Call Conditionlimite()

        'Calcul de la matrice de rigidite global inverse K-1
        Dim KGlobalInverse(,) As Double = CalculeRigiditeInverse()

        Dim ChargeTotale As Double = 0
        Dim Increment As Double
        Dim BodyLoads() As Double
        Dim DeplacementTotal(Nq) As Double
        Dim EncienDeplacement(Nq) As Double
        For Each ElE In Elements
            ElE.InitiStress(Ngp)
        Next

        If Force.Valeur > 0 Then Increment = 10
        If Force.Valeur < 0 Then Increment = -10
        Do While ChargeTotale <> Force.Valeur

            ChargeTotale += Increment
            If Abs(ChargeTotale) > Abs(Force.Valeur) Then
                Increment = Increment - (ChargeTotale - Force.Valeur)
                ChargeTotale = Force.Valeur
            End If

            For Each ElE In Elements
                ElE.InitiStrain(Ngp)
            Next
            Forces = EvaluateExternalForce(Increment)
            Dim Iter As Integer = 0
            'Evalute Initail Force
            Dim Converg As Boolean
            Do
                Iter += 1
                'Trouver les deplacement
                Deplacement = MatriceVecteurMultipe(KGlobalInverse, Forces)
                'Verifier la convergance
                Converg = Convergence(Deplacement, EncienDeplacement, 0.001, Iter)

                ReDim BodyLoads(Nq)

                ' Calcul des contraintes et déformations
                For j = 0 To Elements.Count - 1
                    Stress(ChargeTotale, j, 1, 0, Converg, Iter, MaxIteration, samp, DT) ' 1: comportement non liniaire 0: loi de comportement concrete damaged plasticity
                    Assemblage(BodyLoads, Elements.Item(j).Bload, Elements.Item(j))
                Next
                Forces = VecAdd(Forces, BodyLoads)

            Loop While (Converg <> True And Iter <> MaxIteration)
            ' MsgBox(Iter & "    " & ChargeTotale)
            For i = 0 To Nq
                DeplacementTotal(i) += Deplacement(i)
            Next
            '     DeplacementTotal = VecAdd(DeplacementTotal, Deplacement)
            For j = 0 To Elements.Count - 1
                Dim NodePosition As Integer = ColoserNodeZposition(j)
                Elements.Item(j).ChargerStressStrainCurve()
                Elements.Item(j).ChargerLoadDisplacementCurve(ChargeTotale, DeplacementTotal(NodePosition))
            Next

        Loop
        ReDim Deplacement(Nq)
        For i = 0 To Nq
            Deplacement(i) = DeplacementTotal(i)
        Next
    End Sub
    Private Function EvaluateExternalForce(Charge As Double) As Double()
        Dim FrontElement As Boolean
        Dim Resulats(Nq) As Double
        For j = 0 To Elements.Count - 1
            ' FORMULATION DE VECTEUR DES FORCES
            FrontElement = False
            If j <= Elements.Count - 1 And j > Elements.Count - QuadMesh.Count - 1 Then FrontElement = True
            If FrontElement Then
                Dim Fe() As Double = VecteurForceElementaire(j, Force, Charge, FrontElement)
                ' Assemblage de vecteur des forces
                Assemblage(Resulats, Fe, Elements.Item(j))
            End If
        Next
        Return Resulats
    End Function
    Private Function CheckConvergence(IterVect() As Double, IncremVect() As Double, tol As Double) As Boolean

        Dim MaxIterVect As Double = Max(Abs(IterVect.Max), Abs(IterVect.Min))
        Dim MaxIncremVect As Double = Max(Abs(IncremVect.Max), Abs(IncremVect.Min))
        '  MsgBox(MaxIterVect & "   " & MaxIncremVect)
        If Abs(MaxIterVect) < tol Then Return True
        If (Abs(MaxIterVect) / Abs(MaxIncremVect)) <= tol Then
            Return True
        Else
            Return False
        End If

        Exit Function
        ' Methode des normes
        Dim NormeIterVect As Double = NormeVecteur(IterVect)
        Dim NormeIncremVect As Double = NormeVecteur(IncremVect)

        If (NormeIncremVect / NormeIterVect) <= tol Then
            Return True
        Else
            Return False
        End If

    End Function

    Public Sub ResoudreProblemeNonLineaireInitialStress()
        If PropretyBeton.E = 0 Then PropretyBeton.E = ModuleElasticite(PropretyBeton.Fck)
        Dim LoiComportement As Integer = 0
        If LoiComportement = 1 Then PropretyBeton.E = 450

        If TypeElement = "Cylindre" Then
            Dim Res As MsgBoxResult = MsgBox("Non-conforming mesh, unable to model the element with PDM. Do you want to continue ?", vbAbort, "Concrete v2.0.0")
            If Res <> MsgBoxResult.Yes Then Exit Sub

        End If

            Dim i As Integer
        Nq = 3 * Noeuds.Count - 1
        ' les conditions limites
        Dim MaxIteration As Integer = 100
        IndexNoued.Clear()

        For i = 0 To Nq
            Dim Ind As Integer = i
            IndexNoued.Add(Ind)
        Next

        SurfaceTotal = 0
        For i = 0 To Elements.Count - 1
            If i <= Elements.Count - 1 And i > Elements.Count - QuadMesh.Count - 1 Then
                SurfaceTotal += Surface(i)
            End If
        Next
        Call Conditionlimite()
        'Calcul de la matrice de rigidite global inverse K-1
        Dim KGlobalInverse(,) As Double
        Dim ChargeTotale As Double = 0
        Dim BodyLoads(Nq) As Double
        Dim FrontElement As Boolean
        Dim EncienDeplacement(Nq) As Double
        Dim DeplacementTotal(Nq) As Double

        For Each ElE In Elements
            ElE.InitiStress(Ngp)
            ElE.InitiStrain(Ngp)
            ElE.LoadDisplacement.Clear()
            ElE.CompressiveStressStrain.Clear()
            ElE.TensileStressStrain.Clear()
            '  ElE.Material.PhiDegre = 30
            ElE.Material.E0 = PropretyBeton.E
            ElE.Material.PsiDegre = PropretyBeton.PsiDegre
        Next
        ChargeTotale = 0
        MsgBox("The sample will be modeled according to fcm = " & (Elements.Item(0).Material.Fck + 8).ToString & " MPa")
        'Compression
        Dim Iter As Integer
        Dim Converg As Boolean
        KGlobalInverse = CalculeRigiditeInverse()
        Dim Increments As New List(Of Single)
        If CompressiveCase Then
            Increments.Add(-5)
            For i = 1 To 200
                Increments.Add(-1)
            Next
            'ChargeTotale = -5
        Else
            ' ChargeTotale = 1
            Dim Ftm As Double = 0.3016 * PropretyBeton.Fck ^ (2 / 3)
            Increments.Add(0.8 * Ftm)
            Increments.Add(0.1 * Ftm)
            Increments.Add(0.1 * Ftm)

            '  Increments.Add(0.05 * Ftm)
            For i = 1 To 100
                Increments.Add(0.3 * Ftm)
            Next
        End If
        Dim MaxDamage As Double
        For Each Incr In Increments
            MaxDamage = 0
            ChargeTotale += Incr
            '  MsgBox(ChargeTotale)
            Iter = 0
            Converg = False
            ReDim Forces(Nq)

            'Calcul de Force initial
            For j = 0 To Elements.Count - 1
                ' FORMULATION DE VECTEUR DES FORCES
                FrontElement = False
                If j <= Elements.Count - 1 And j > Elements.Count - QuadMesh.Count - 1 Then FrontElement = True
                If FrontElement Then
                    Dim Fe() As Double = VecteurForceElementaire(j, Force, Incr, FrontElement)
                    ' Assemblage de vecteur des forces
                    Assemblage(Forces, Fe, Elements.Item(j))
                End If
            Next
            Dim IncForces(Nq) As Double
            For i = 0 To Nq
                IncForces(i) = Forces(i)
            Next

            Dim TotalForces(Nq) As Double

            '      TotalForces = VecAdd(Forces, BodyLoads, -1)
            TotalForces = CopyVector(Forces)
            Do
                Iter += 1
                ReDim BodyLoads(Nq)
                'Trouver les deplacement
                Deplacement = MatriceVecteurMultipe(KGlobalInverse, TotalForces)

                For j = 0 To Elements.Count - 1
                    If j = 16 Then
                        j += 0
                    End If
                    Stress(ChargeTotale, j, 2, LoiComportement, Converg, Iter, MaxIteration, samp) ' 0: comportement non liniaire 1: Initial Strain  2: Initial Stress
                    Assemblage(BodyLoads, Elements.Item(j).Bload, Elements.Item(j))
                Next

                TotalForces = CopyVector(BodyLoads)
                Converg = CheckConvergence(BodyLoads, IncForces, 0.01)
                For i = 0 To Nq
                    DeplacementTotal(i) += Deplacement(i)
                Next
                If Converg = True Then Exit Do
                If Iter = MaxIteration Then Exit Do
            Loop
            MaxDamage = 0
            For j = 0 To Elements.Count - 1
                Dim NodePosition As Integer = ColoserNodeZposition(j)
                If MaxDamage < MoyenneTenseur(Elements.Item(j).DamageParameter) Then MaxDamage = MoyenneTenseur(Elements.Item(j).DamageParameter)
                Elements.Item(j).ChargerStressStrainCurve()
                Elements.Item(j).ChargerLoadDisplacementCurve(ChargeTotale, DeplacementTotal(NodePosition))
            Next
            '  MsgBox("Iteration = " & Iter & "  Charge = " & ChargeTotale)
            If MaxDamage > 0.92 Then Exit For
        Next
        For i = 0 To Nq
            Deplacement(i) = DeplacementTotal(i)
        Next
        MsgBox("The calculation is finished")
    End Sub

    Private Function ColoserNodeZposition(Optional ByRef IndexElement As Integer = -1) As Integer
        If IndexElement = -1 Then
            Dim Dist As Double = 1000000000000000
            Dim ii As Integer
            For j = 0 To Elements.Count - 1
                For i = 0 To Elements.Item(j).ListNoeud.Count - 1
                    If Elements.Item(j).ListNoeud(i).Coord(3) = Haut Then
                        Dim distan As Double = Distance(Longeur / 2, Diametre / 2, Elements.Item(j).ListNoeud(i).Coord(1), Elements.Item(j).ListNoeud(i).Coord(2))
                        If distan < Dist Then
                            ii = 3 * Elements.Item(j).ListNoeud(i).Ident + 2
                            IndexElement = j
                            ii = IndexNoued.Item(ii)
                            Dist = distan
                        End If
                    End If
                Next
            Next
            Return ii
        Else

            Dim ii As Integer
            Dim J As Integer = IndexElement
            ii = 3 * Elements.Item(J).ListNoeud(4).Ident + 2
            IndexElement = J
            ii = IndexNoued.Item(ii)



            Return ii
        End If

    End Function

    Public Sub DrawComprissiveStressStrainCurve(ByRef E As Graphics, W As Single, H As Single, Xmax As Double, Ymax As Double, IndexElement As Integer, WidthPen As Integer)
        If Xmax = 0 Or Ymax = 0 Then Exit Sub
        Dim DrawSize As Size = New Size With {.Height = 1000, .Width = 1000}
        Dim EcheX As Single = W / (DrawSize.Width)
        Dim EcheY As Single = H / (DrawSize.Height)
        Dim Dy As Single = 0.05 * DrawSize.Height * EcheY
        Dim Dx As Single = 0.05 * DrawSize.Width * EcheX
        Dim PenGrid As New Pen(Color.Gray)
        Dim PenGraph As New Pen(Color.Blue)
        Dim ELE As BrickEightNodes
        If IndexElement <= Elements.Count - 1 Then
            ELE = Elements.Item(IndexElement)
        Else
            MsgBox("Impossible de trouver cet élément")
            Exit Sub
        End If
        E.TranslateTransform(Dx, Dy)
        Dim Font As New Font("Arial", 7)

        Dim Text As String
        Dim PhSize As SizeF

        Dim XEc As Double = 850 * EcheX / Xmax
        Dim YEc As Double = 850 * EcheY / Ymax

        'tracer les axes
        PenGrid.Width = WidthPen
        PenGrid.Color = Color.Red
        E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX, .Y = 0}, New PointF With {.X = 50 * EcheX, .Y = 850 * EcheY})
        E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX, .Y = 850 * EcheY}, New PointF With {.X = 900 * EcheX, .Y = 850 * EcheY})


        For i = 1 To 10
            E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX, .Y = 850 * EcheY - (i * Ymax / 10) * YEc},
                       New PointF With {.X = 900 * EcheX, .Y = 850 * EcheY - (i * Ymax / 10) * YEc})
            E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX + (i * Xmax / 10) * XEc, .Y = 0},
              New PointF With {.X = 50 * EcheX + (i * Xmax / 10) * XEc, .Y = 850 * EcheY})

            Text = RendreString(i * Ymax / 10, 1)
            PhSize = E.MeasureString(Text, Font)
            E.DrawString(Text, Font, New SolidBrush(Color.Black),
                         New PointF With {.X = 50 * EcheX - PhSize.Width - 3, .Y = 850 * EcheY - (i * Ymax / 10) * YEc - PhSize.Height / 2}) '+ PhSize.Height / 2

            Text = RendreString(i * Xmax / 10, 4)
            PhSize = E.MeasureString(Text, Font)
            E.DrawString(Text, Font, New SolidBrush(Color.Black),
                         New PointF With {.X = 50 * EcheX + (i * Xmax / 10) * XEc - PhSize.Width / 2, .Y = 850 * EcheY + PhSize.Height + 3}) '+ PhSize.Height / 2



        Next
        PenGrid.Width = WidthPen
        PenGrid.Color = Color.Blue
        PenGrid.DashStyle = Drawing2D.DashStyle.Solid
        If ELE.CompressiveStressStrain.Count >= 2 Then
            For i = 0 To ELE.CompressiveStressStrain.Count - 2
                If i = 0 Then
                    E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX + Abs(ELE.CompressiveStressStrain.Item(i).x) * XEc, .Y = 850 * EcheY - ELE.CompressiveStressStrain.Item(i).y * YEc},
                                      New PointF With {.X = 50 * EcheX, .Y = 850 * EcheY})
                    '  E.FillRectangle(New SolidBrush(Color.BlueViolet), New Drawing.Rectangle With {.Height = 6, .Width = 6,
                    '.Location = New Drawing.Point With {.X = 50 * EcheX + Abs(ELE.CompressiveStressStrain.Item(i).x) * XEc - 3, .Y = 850 * EcheY - ELE.CompressiveStressStrain.Item(i).y * YEc - 3}})

                End If
                E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX + Abs(ELE.CompressiveStressStrain.Item(i).x) * XEc, .Y = 850 * EcheY - ELE.CompressiveStressStrain.Item(i).y * YEc},
                       New PointF With {.X = 50 * EcheX + Abs(ELE.CompressiveStressStrain.Item(i + 1).x) * XEc, .Y = 850 * EcheY - ELE.CompressiveStressStrain.Item(i + 1).y * YEc})

                '     E.FillRectangle(New SolidBrush(Color.BlueViolet), New Drawing.Rectangle With {.Height = 6, .Width = 6,
                '.Location = New Drawing.Point With {.X = 50 * EcheX + Abs(ELE.CompressiveStressStrain.Item(i + 1).x) * XEc - 3, .Y = 850 * EcheY - ELE.CompressiveStressStrain.Item(i + 1).y * YEc - 3}})
            Next
        ElseIf ELE.CompressiveStressStrain.Count = 1 Then
            E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX, .Y = 850 * EcheY},
                      New PointF With {.X = 50 * EcheX + Abs(ELE.CompressiveStressStrain.Item(0).x) * XEc, .Y = 850 * EcheY - ELE.CompressiveStressStrain.Item(0).y * YEc})
            E.FillRectangle(New SolidBrush(Color.BlueViolet), New Drawing.Rectangle With {.Height = 6, .Width = 6,
                        .Location = New Drawing.Point With {.X = 50 * EcheX + Abs(ELE.CompressiveStressStrain.Item(0).x) * XEc - 3, .Y = 850 * EcheY - ELE.CompressiveStressStrain.Item(0).y * YEc - 3}})
        End If


        PenGrid.Width = 1
        PenGrid.Color = Color.Red

        'Titre de courbe

        Text = "Fcm= " & (PropretyBeton.Fck + 8).ToString & " MPa, " & "Ftm= " & RendreString(0.3016 * PropretyBeton.Fck ^ (2 / 3), 2) & " MPa, " & "E0= " & RendreString(PropretyBeton.E.ToString, 0) & " MPa."
        PhSize = E.MeasureString(Text, Font)

        E.FillRectangle(New SolidBrush(Color.White), New Drawing.Rectangle With {.Height = 40, .Width = 330, .Location = New Drawing.Point With {.X = 80 * EcheX - 5, .Y = -15}})
        E.DrawRectangle(PenGrid, New Drawing.Rectangle With {.Height = 40, .Width = 330, .Location = New Drawing.Point With {.X = 80 * EcheX - 5, .Y = -15}})


        E.DrawString(Text, New Font("Arial", 10), New SolidBrush(Color.Black), New PointF With {.X = 100 * EcheX, .Y = PhSize.Height - 7}) ' / 2

        Text = "Compressive σ-ε --> Element " & (IndexElement + 1).ToString & " / " & OurProblem.Elements.Count
        PhSize = E.MeasureString(Text, Font)
        E.DrawString(Text, New Font("Arial", 10), New SolidBrush(Color.Black), New PointF With {.X = 250 * EcheX, .Y = -15}) '+ PhSize.Height / 2



        PenGrid.Width = WidthPen
    End Sub

    Public Sub DrawTensileStressStrainCurve(ByRef E As Graphics, W As Single, H As Single, Xmax As Double, Ymax As Double, IndexElement As Integer, WidthPen As Integer)
        If Xmax = 0 Or Ymax = 0 Then Exit Sub
        Dim DrawSize As Size = New Size With {.Height = 1000, .Width = 1000}
        Dim EcheX As Single = W / (DrawSize.Width)
        Dim EcheY As Single = H / (DrawSize.Height)
        Dim Dy As Single = 0.05 * DrawSize.Height * EcheY
        Dim Dx As Single = 0.05 * DrawSize.Width * EcheX
        Dim PenGrid As New Pen(Color.Gray)
        Dim PenGraph As New Pen(Color.Blue)
        Dim ELE As BrickEightNodes
        If IndexElement <= Elements.Count - 1 Then
            ELE = Elements.Item(IndexElement)
        Else
            MsgBox("Impossible de trouver cet élément")
            Exit Sub
        End If
        E.TranslateTransform(Dx, Dy)
        Dim Font As New Font("Arial", 7)

        Dim Text As String
        Dim PhSize As SizeF

        Dim XEc As Double = 850 * EcheX / Xmax
        Dim YEc As Double = 850 * EcheY / Ymax

        'tracer les axes
        PenGrid.Width = WidthPen
        PenGrid.Color = Color.Red
        E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX, .Y = 0}, New PointF With {.X = 50 * EcheX, .Y = 850 * EcheY})
        E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX, .Y = 850 * EcheY}, New PointF With {.X = 900 * EcheX, .Y = 850 * EcheY})

        PenGrid.Width = 1
        PenGrid.Color = Color.Red

        PenGrid.DashStyle = Drawing2D.DashStyle.Dash
        For i = 1 To 10
            E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX, .Y = 850 * EcheY - (i * Ymax / 10) * YEc},
                       New PointF With {.X = 900 * EcheX, .Y = 850 * EcheY - (i * Ymax / 10) * YEc})
            E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX + (i * Xmax / 10) * XEc, .Y = 0},
              New PointF With {.X = 50 * EcheX + (i * Xmax / 10) * XEc, .Y = 850 * EcheY})

            Text = RendreString(i * Ymax / 10, 1)
            PhSize = E.MeasureString(Text, Font)
            E.DrawString(Text, Font, New SolidBrush(Color.Black),
                         New PointF With {.X = 50 * EcheX - PhSize.Width - 3, .Y = 850 * EcheY - (i * Ymax / 10) * YEc - PhSize.Height / 2}) '+ PhSize.Height / 2

            Text = RendreString(i * 1000 * Xmax / 10, 4)
            PhSize = E.MeasureString(Text, Font)
            E.DrawString(Text, Font, New SolidBrush(Color.Black),
                         New PointF With {.X = 50 * EcheX + (i * Xmax / 10) * XEc - PhSize.Width / 2, .Y = 850 * EcheY + PhSize.Height + 3}) '+ PhSize.Height / 2

        Next
        PenGrid.Width = WidthPen
        PenGrid.Color = Color.Blue
        PenGrid.DashStyle = Drawing2D.DashStyle.Solid
        For i = 0 To ELE.TensileStressStrain.Count - 2
            If i = 0 Then
                E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX + Abs(ELE.TensileStressStrain.Item(i).x) * XEc, .Y = 850 * EcheY - ELE.TensileStressStrain.Item(i).y * YEc},
                                      New PointF With {.X = 50 * EcheX, .Y = 850 * EcheY})
                'E.FillRectangle(New SolidBrush(Color.BlueViolet), New Drawing.Rectangle With {.Height = 6, .Width = 6,
                '.Location = New Drawing.Point With {.X = 50 * EcheX + Abs(ELE.TensileStressStrain.Item(i).x) * XEc - 3, .Y = 850 * EcheY - ELE.TensileStressStrain.Item(i).y * YEc - 3}})

            End If
            E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX + Abs(ELE.TensileStressStrain.Item(i).x) * XEc, .Y = 850 * EcheY - ELE.TensileStressStrain.Item(i).y * YEc},
                       New PointF With {.X = 50 * EcheX + Abs(ELE.TensileStressStrain.Item(i + 1).x) * XEc, .Y = 850 * EcheY - ELE.TensileStressStrain.Item(i + 1).y * YEc})



            'E.FillRectangle(New SolidBrush(Color.BlueViolet), New Drawing.Rectangle With {.Height = 6, .Width = 6,
            '.Location = New Drawing.Point With {.X = 50 * EcheX + Abs(ELE.TensileStressStrain.Item(i + 1).x) * XEc - 3, .Y = 850 * EcheY - ELE.TensileStressStrain.Item(i + 1).y * YEc - 3}})

        Next



        PenGrid.Width = 1
        PenGrid.Color = Color.Red


        Text = "Fcm= " & (PropretyBeton.Fck + 8).ToString & " MPa, " & "Ftm= " & RendreString(0.3016 * PropretyBeton.Fck ^ (2 / 3), 2) & " MPa, " & "E0= " & RendreString(PropretyBeton.E.ToString, 0) & " MPa."
        PhSize = E.MeasureString(Text, Font)

        E.FillRectangle(New SolidBrush(Color.White), New Drawing.Rectangle With {.Height = 40, .Width = 330, .Location = New Drawing.Point With {.X = 80 * EcheX - 5, .Y = -15}})
        E.DrawRectangle(PenGrid, New Drawing.Rectangle With {.Height = 40, .Width = 330, .Location = New Drawing.Point With {.X = 80 * EcheX - 5, .Y = -15}})


        E.DrawString(Text, New Font("Arial", 10), New SolidBrush(Color.Black), New PointF With {.X = 100 * EcheX, .Y = PhSize.Height - 7}) ' / 2

        Text = "Tensile σ-ε --> Element " & (IndexElement + 1).ToString & " / " & OurProblem.Elements.Count
        PhSize = E.MeasureString(Text, Font)
        E.DrawString(Text, New Font("Arial", 10), New SolidBrush(Color.Black), New PointF With {.X = 250 * EcheX, .Y = -15}) '+ PhSize.Height / 2


    End Sub
    Public Sub DrawComprissiveCDPMCurveAlfarah(ByRef E As Graphics, W As Single, H As Single, Xmax As Double, Ymax As Double, IndexElement As Integer, WidthPen As Integer)
        If Xmax = 0 Or Ymax = 0 Then Exit Sub
        Dim DrawSize As Size = New Size With {.Height = 1000, .Width = 1000}
        Dim EcheX As Single = W / (DrawSize.Width)
        Dim EcheY As Single = H / (DrawSize.Height)
        Dim Dy As Single = 0.05 * DrawSize.Height * EcheY
        Dim Dx As Single = 0.05 * DrawSize.Width * EcheX
        Dim PenGrid As New Pen(Color.Gray)
        Dim PenGraph As New Pen(Color.Blue)
        Dim ELE As BrickEightNodes
        If IndexElement <= Elements.Count - 1 Then
            ELE = Elements.Item(IndexElement)
        Else
            MsgBox("Impossible de trouver cet élément")
            Exit Sub
        End If
        E.TranslateTransform(Dx, Dy)
        Dim Font As New Font("Arial", 7)

        Dim Text As String
        Dim PhSize As SizeF

        Dim XEc As Double = 850 * EcheX / Xmax
        Dim YEc As Double = 850 * EcheY / Ymax

        'tracer les axes
        PenGrid.Width = WidthPen
        PenGrid.Color = Color.Gray
        E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX, .Y = 0}, New PointF With {.X = 50 * EcheX, .Y = 850 * EcheY})
        E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX, .Y = 850 * EcheY}, New PointF With {.X = 900 * EcheX, .Y = 850 * EcheY})

        PenGrid.Width = 1
        For i = 1 To 10
            E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX, .Y = 850 * EcheY - (i * Ymax / 10) * YEc},
                       New PointF With {.X = 900 * EcheX, .Y = 850 * EcheY - (i * Ymax / 10) * YEc})
            E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX + (i * Xmax / 10) * XEc, .Y = 0},
              New PointF With {.X = 50 * EcheX + (i * Xmax / 10) * XEc, .Y = 850 * EcheY})

            Text = RendreString(i * Ymax / 10, 1)
            PhSize = E.MeasureString(Text, Font)
            E.DrawString(Text, Font, New SolidBrush(Color.Black),
                         New PointF With {.X = 50 * EcheX - PhSize.Width - 3, .Y = 850 * EcheY - (i * Ymax / 10) * YEc - PhSize.Height / 2}) '+ PhSize.Height / 2

            Text = RendreString(i * Xmax / 10, 4)
            PhSize = E.MeasureString(Text, Font)
            E.DrawString(Text, Font, New SolidBrush(Color.Black),
                         New PointF With {.X = 50 * EcheX + (i * Xmax / 10) * XEc - PhSize.Width / 2, .Y = 850 * EcheY + PhSize.Height + 1}) '+ PhSize.Height / 2



        Next
        PenGrid.Width = WidthPen
        PenGrid.Color = Color.Blue
        PenGrid.DashStyle = Drawing2D.DashStyle.Solid
        Dim Fck As Double = ELE.Material.Fck
        Dim Leq As Double = 200 ' ELE.Leq
        Dim Hc, Dc, DefPlasC, OldDefPlasC, SigmaC, OldSigmaC As Double

        ' DefPlasC = 0.000005 * i
        Dim LeqMtr(4) As Double
        LeqMtr(1) = 300
        LeqMtr(2) = 100
        LeqMtr(3) = 200
        LeqMtr(4) = 400
        Dim OurColor(4) As Color
        OurColor(1) = Color.Blue
        OurColor(2) = Color.DarkBlue
        OurColor(3) = Color.Red
        OurColor(4) = Color.DarkRed
        For jj = 1 To 1
            Dc = 0
            Leq = LeqMtr(jj)
            PenGrid.Color = OurColor(jj)
            OldDefPlasC = 0
            OldSigmaC = 0

            Dim Fcm, Fc0, Ft0, Ftm, Defcm, Deftm, Eci, E0, Gf, Gch, Wc, Id As Double
            Id = 0
            Fcm = Fck + 8
            Ftm = 0.3016 * Fck ^ (2 / 3)
            Defcm = 0.0007 * Fcm ^ 0.31
            Eci = 10000 * Fcm ^ (1 / 3)
            E0 = Eci * (0.8 + 0.2 * (Fcm / 88))
            Gf = 0.073 * Fcm ^ 0.18
            Gch = Gf * (Fcm / Ftm) ^ 2
            Wc = 5.14 * Gf / Ftm
            Deftm = Ftm / E0
            Fc0 = 0.4 * Fcm
            Ft0 = Ftm
            Dim ac, at, bc, bt, DefCpl, b, Newb As Double
            ac = 7.873
            at = 1
            bc = (1.97 * Fcm / Gch) * Leq
            bt = (0.453 * Fck ^ (2 / 3) / Gf) * Leq



            ' Dc = 1 / (2 + ac)
            '  Dc = Dc * (2 * (1 + ac) * Exp(-bc * DefPlasC) - ac * Exp(-2 * bc * DefPlasC))
            '  Dc = 1 - Dc
            ' SigmaC = Fc0 * ((1 + ac) * Exp(-bc * DefPlasC) - ac * Exp(-2 * bc * DefPlasC))
            Do Until Dc > 0.995
                Id = Id + 5

                Dim DefC As Double = 0.000008 * Id
                Dim iter As Integer = 0
                Dim ElaticDefor As Double = 0.4 * Fcm / E0
                b = 0.9
                Newb = 0
                Do 'Lancer iteration
                    iter += 1
                    If DefC >= 0 And DefC <= ElaticDefor Then
                        SigmaC = SigmaCI(DefC, E0)
                        DefPlasC = 0
                    ElseIf DefC > ElaticDefor And DefC <= Defcm Then
                        SigmaC = SigmaCII(DefC, Defcm, Fcm, Eci)
                        DefPlasC = DefC - SigmaC / E0
                    ElseIf DefC > Defcm Then
                        SigmaC = SigmaCIII(DefC, Defcm, Fcm, Gch, Leq, b, E0)
                        DefPlasC = DefC - SigmaC / E0
                        Dc = 1 / (2 + ac)
                        Dc = Dc * (2 * (1 + ac) * Exp(-bc * DefPlasC) - ac * Exp(-2 * bc * DefPlasC))
                        Dc = 1 - Dc
                        ' If Dc > 0.1 And Dc < 0.2 Then MsgBox(DefPlasC)
                    End If
                    DefCpl = DefPlasC - SigmaC * Dc / (E0 * (1 - Dc))
                    If DefCpl < 0 Then
                        DefCpl = 0
                    End If

                    If DefPlasC <> 0 Then
                        Newb = DefCpl / DefPlasC
                        If Newb < 0 Then
                            Newb += 0
                        End If
                        If Convergence(Newb, b, 0.0001) Or iter > 400 Then
                            Exit Do
                        End If
                        b = Newb
                    Else
                        Exit Do
                    End If
                Loop
                If OldDefPlasC <> 0 And OldSigmaC <> 0 Then
                    E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX + OldDefPlasC * XEc, .Y = 850 * EcheY - OldSigmaC * YEc},
                           New PointF With {.X = 50 * EcheX + DefC * XEc, .Y = 850 * EcheY - SigmaC * YEc})
                End If
                '  MsgBox("DefCpl =" & DefC & "    SigmaC=  " & SigmaC)
                OldDefPlasC = DefC
                OldSigmaC = SigmaC
            Loop

        Next

        PenGrid.Width = 1
        PenGrid.Color = Color.Black

        'Titre de courbe

        Text = "Fcm= " & (PropretyBeton.Fck + 8).ToString & " MPa, " & "Ftm= " & RendreString(0.3016 * PropretyBeton.Fck ^ (2 / 3), 2) & " MPa, " & "E0= " & RendreString(PropretyBeton.E.ToString, 0) & " MPa."
        PhSize = E.MeasureString(Text, Font)

        E.FillRectangle(New SolidBrush(Color.White), New Drawing.Rectangle With {.Height = 40, .Width = 330, .Location = New Drawing.Point With {.X = 80 * EcheX - 5, .Y = -15}})
        E.DrawRectangle(PenGrid, New Drawing.Rectangle With {.Height = 40, .Width = 330, .Location = New Drawing.Point With {.X = 80 * EcheX - 5, .Y = -15}})


        E.DrawString(Text, New Font("Arial", 10), New SolidBrush(Color.Black), New PointF With {.X = 100 * EcheX, .Y = PhSize.Height - 7}) ' / 2

        Text = "Compressive σ-ε --> Element " & (IndexElement + 1).ToString & " / " & OurProblem.Elements.Count
        PhSize = E.MeasureString(Text, Font)
        E.DrawString(Text, New Font("Arial", 10), New SolidBrush(Color.Black), New PointF With {.X = 250 * EcheX, .Y = -15}) '+ PhSize.Height / 2


    End Sub
    Public Sub DrawComprissiveDamageParameterAlfarah(ByRef E As Graphics, W As Single, H As Single, Xmax As Double, Ymax As Double, IndexElement As Integer, WidthPen As Integer)
        If Xmax = 0 Or Ymax = 0 Then Exit Sub
        Dim DrawSize As Size = New Size With {.Height = 1000, .Width = 1000}
        Dim EcheX As Single = W / (DrawSize.Width)
        Dim EcheY As Single = H / (DrawSize.Height)
        Dim Dy As Single = 0.05 * DrawSize.Height * EcheY
        Dim Dx As Single = 0.05 * DrawSize.Width * EcheX
        Dim PenGrid As New Pen(Color.Gray)
        Dim PenGraph As New Pen(Color.Blue)
        Dim ELE As BrickEightNodes
        If IndexElement <= Elements.Count - 1 Then
            ELE = Elements.Item(IndexElement)
        Else
            MsgBox("Impossible de trouver cet élément")
            Exit Sub
        End If
        E.TranslateTransform(Dx, Dy)
        Dim Font As New Font("Arial", 7)

        Dim Text As String
        Dim PhSize As SizeF

        Dim XEc As Double = 850 * EcheX / Xmax
        Dim YEc As Double = 850 * EcheY / Ymax

        'tracer les axes
        PenGrid.Width = WidthPen
        PenGrid.Color = Color.Gray
        E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX, .Y = 0}, New PointF With {.X = 50 * EcheX, .Y = 850 * EcheY})
        E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX, .Y = 850 * EcheY}, New PointF With {.X = 900 * EcheX, .Y = 850 * EcheY})

        PenGrid.Width = 1



        For i = 1 To 10
            E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX, .Y = 850 * EcheY - (i * Ymax / 10) * YEc},
                       New PointF With {.X = 900 * EcheX, .Y = 850 * EcheY - (i * Ymax / 10) * YEc})
            E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX + (i * Xmax / 10) * XEc, .Y = 0},
              New PointF With {.X = 50 * EcheX + (i * Xmax / 10) * XEc, .Y = 850 * EcheY})

            Text = RendreString(i * Ymax / 10, 1)
            PhSize = E.MeasureString(Text, Font)
            E.DrawString(Text, Font, New SolidBrush(Color.Black),
                         New PointF With {.X = 50 * EcheX - PhSize.Width - 3, .Y = 850 * EcheY - (i * Ymax / 10) * YEc - PhSize.Height / 2}) '+ PhSize.Height / 2

            Text = RendreString(i * Xmax / 10, 4)
            PhSize = E.MeasureString(Text, Font)
            E.DrawString(Text, Font, New SolidBrush(Color.Black),
                         New PointF With {.X = 50 * EcheX + (i * Xmax / 10) * XEc - PhSize.Width / 2, .Y = 850 * EcheY + PhSize.Height + 3}) '+ PhSize.Height / 2



        Next
        PenGrid.Width = WidthPen
        PenGrid.Color = Color.Blue
        PenGrid.DashStyle = Drawing2D.DashStyle.Solid
        Dim Fck As Double = ELE.Material.Fck
        Dim Leq As Double = 100 ' ELE.Leq
        Dim Hc, Dc, DefPlasC, OldDefPlasC, SigmaC, OldSigmaC As Double

        ' DefPlasC = 0.000005 * i
        Dim LeqMtr(4) As Double
        LeqMtr(1) = 50
        LeqMtr(2) = 100
        LeqMtr(3) = 200
        LeqMtr(4) = 400
        Dim OurColor(4) As Color
        OurColor(1) = Color.Blue
        OurColor(2) = Color.DarkBlue
        OurColor(3) = Color.Red
        OurColor(4) = Color.DarkRed
        For jj = 1 To 4
            Dc = 0
            Leq = LeqMtr(jj)
            PenGrid.Color = OurColor(jj)
            OldDefPlasC = 0
            OldSigmaC = 0

            Dim Fcm, Fc0, Ft0, Ftm, Defcm, Deftm, Eci, E0, Gf, Gch, Wc, Id As Double
            Id = 0
            Fcm = Fck + 8
            Ftm = 0.3016 * Fck ^ (2 / 3)
            Defcm = 0.0007 * Fcm ^ 0.31
            Eci = 10000 * Fcm ^ (1 / 3)
            E0 = Eci * (0.8 + 0.2 * (Fcm / 88))
            Gf = 0.073 * Fcm ^ 0.18
            Gch = Gf * (Fcm / Ftm) ^ 2
            Wc = 5.14 * Gf / Ftm
            Deftm = Ftm / E0
            Fc0 = 0.4 * Fcm
            Ft0 = Ftm
            Dim ac, at, bc, bt, DefCpl, b, Newb As Double
            ac = 7.873
            at = 1
            bc = (1.97 * Fcm / Gch) * Leq
            bt = (0.453 * Fck ^ (2 / 3) / Gf) * Leq



            ' Dc = 1 / (2 + ac)
            '  Dc = Dc * (2 * (1 + ac) * Exp(-bc * DefPlasC) - ac * Exp(-2 * bc * DefPlasC))
            '  Dc = 1 - Dc
            ' SigmaC = Fc0 * ((1 + ac) * Exp(-bc * DefPlasC) - ac * Exp(-2 * bc * DefPlasC))
            Do Until Dc > 0.999995
                Id = Id + 5

                Dim DefC As Double = 0.000008 * Id
                Dim iter As Integer = 0
                Dim ElaticDefor As Double = 0.4 * Fcm / E0
                b = 0.9
                Newb = 0
                Do 'Lancer iteration
                    iter += 1
                    If DefC >= 0 And DefC <= ElaticDefor Then
                        SigmaC = SigmaCI(DefC, E0)
                        DefPlasC = 0
                    ElseIf DefC > ElaticDefor And DefC <= Defcm Then
                        SigmaC = SigmaCII(DefC, Defcm, Fcm, Eci)
                        DefPlasC = DefC - SigmaC / E0
                    ElseIf DefC > Defcm Then
                        SigmaC = SigmaCIII(DefC, Defcm, Fcm, Gch, Leq, b, E0)
                        DefPlasC = DefC - SigmaC / E0
                        ' If Dc > 0.1 And Dc < 0.2 Then MsgBox(DefPlasC)
                    End If
                    Dc = 1 / (2 + ac)
                    Dc = Dc * (2 * (1 + ac) * Exp(-bc * DefPlasC) - ac * Exp(-2 * bc * DefPlasC))
                    Dc = 1 - Dc
                    DefCpl = DefPlasC - SigmaC * Dc / (E0 * (1 - Dc))
                    If DefCpl < 0 Then
                        DefCpl = 0
                    End If

                    If DefPlasC <> 0 Then
                        Newb = DefCpl / DefPlasC
                        If Newb < 0 Then
                            Newb += 0
                        End If
                        If Convergence(Newb, b, 0.0001) Or iter > 400 Then
                            Exit Do
                        End If
                        b = Newb
                    Else
                        Exit Do
                    End If
                Loop
                If OldDefPlasC <> 0 And OldSigmaC <> 0 Then
                    E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX + OldDefPlasC * XEc, .Y = 850 * EcheY - OldSigmaC * YEc},
                           New PointF With {.X = 50 * EcheX + DefC * XEc, .Y = 850 * EcheY - Dc * YEc})
                End If

                OldDefPlasC = DefC
                OldSigmaC = Dc
            Loop

        Next

        PenGrid.Width = 1
        PenGrid.Color = Color.Black

        'Titre de courbe
        'E.FillRectangle(New SolidBrush(Color.White), New Drawing.Rectangle With {.Height = 20, .Width = 175, .Location = New Drawing.Point With {.X = 300 * EcheX - 5, .Y = -9}})
        'E.DrawRectangle(PenGrid, New Drawing.Rectangle With {.Height = 20, .Width = 175, .Location = New Drawing.Point With {.X = 300 * EcheX - 5, .Y = -9}})
        'Text = "CDP Model Curve"
        'E.DrawString(Text, New Font("Arial", 10), New SolidBrush(Color.Black), New PointF With {.X = 350 * EcheX, .Y = -7}) '+ PhSize.Height / 2
        PenGrid.Width = WidthPen
    End Sub

    Public Sub DrawTensileCDPMCurveAlfarah(ByRef E As Graphics, W As Single, H As Single, Xmax As Double, Ymax As Double, IndexElement As Integer, WidthPen As Integer)
        If Xmax = 0 Or Ymax = 0 Then Exit Sub
        Dim DrawSize As Size = New Size With {.Height = 1000, .Width = 1000}
        Dim EcheX As Single = W / (DrawSize.Width)
        Dim EcheY As Single = H / (DrawSize.Height)
        Dim Dy As Single = 0.05 * DrawSize.Height * EcheY
        Dim Dx As Single = 0.05 * DrawSize.Width * EcheX
        Dim PenGrid As New Pen(Color.Gray)
        Dim PenGraph As New Pen(Color.Blue)
        Dim ELE As BrickEightNodes
        If IndexElement <= Elements.Count - 1 Then
            ELE = Elements.Item(IndexElement)
        Else
            MsgBox("Impossible de trouver cet élément")
            Exit Sub
        End If
        E.TranslateTransform(Dx, Dy)
        Dim Font As New Font("Arial", 7)

        Dim Text As String
        Dim PhSize As SizeF

        Dim XEc As Double = 850 * EcheX / Xmax
        Dim YEc As Double = 850 * EcheY / Ymax

        'tracer les axes
        PenGrid.Width = WidthPen
        PenGrid.Color = Color.Gray
        E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX, .Y = 0}, New PointF With {.X = 50 * EcheX, .Y = 850 * EcheY})
        E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX, .Y = 850 * EcheY}, New PointF With {.X = 900 * EcheX, .Y = 850 * EcheY})

        PenGrid.Width = 1


        For i = 1 To 10
            E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX, .Y = 850 * EcheY - (i * Ymax / 10) * YEc},
                       New PointF With {.X = 900 * EcheX, .Y = 850 * EcheY - (i * Ymax / 10) * YEc})
            E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX + (i * Xmax / 10) * XEc, .Y = 0},
              New PointF With {.X = 50 * EcheX + (i * Xmax / 10) * XEc, .Y = 850 * EcheY})

            Text = RendreString(i * Ymax / 10, 1)
            PhSize = E.MeasureString(Text, Font)
            E.DrawString(Text, Font, New SolidBrush(Color.Black),
                         New PointF With {.X = 50 * EcheX - PhSize.Width - 3, .Y = 850 * EcheY - (i * Ymax / 10) * YEc - PhSize.Height / 2}) '+ PhSize.Height / 2

            Text = RendreString(i * Xmax / 10, 4)
            PhSize = E.MeasureString(Text, Font)
            E.DrawString(Text, Font, New SolidBrush(Color.Black),
                         New PointF With {.X = 50 * EcheX + (i * Xmax / 10) * XEc - PhSize.Width / 2, .Y = 850 * EcheY + PhSize.Height + 3}) '+ PhSize.Height / 2



        Next
        PenGrid.Width = WidthPen
        PenGrid.Color = Color.Blue
        PenGrid.DashStyle = Drawing2D.DashStyle.Solid
        Dim Fck As Double = ELE.Material.Fck
        Dim Leq As Double = ELE.Material.Leq
        Dim Hc, Dt, DefPlasT, OldDefPlasT, SigmaT, OldSigmaT As Double

        ' DefPlasC = 0.000005 * i

        Dim LeqMtr(4) As Double
        LeqMtr(1) = 50
        LeqMtr(2) = 100
        LeqMtr(3) = 200
        LeqMtr(4) = 400
        Dim OurColor(4) As Color
        OurColor(1) = Color.Blue
        OurColor(2) = Color.DarkBlue
        OurColor(3) = Color.Red
        OurColor(4) = Color.DarkRed
        For jj = 1 To 4
            Dt = 0
            Leq = LeqMtr(jj)
            PenGrid.Color = OurColor(jj)
            OldDefPlasT = 0
            OldSigmaT = 0

            Dim Fcm, Fc0, Ft0, Ftm, Defcm, Deftm, Eci, E0, Gf, Gch, Wc, Id As Double
            Id = 0
            Fcm = Fck + 8
            Ftm = 0.3016 * Fck ^ (2 / 3)
            Defcm = 0.0007 * Fcm ^ 0.31
            Eci = 10000 * Fcm ^ (1 / 3)
            E0 = Eci * (0.8 + 0.2 * (Fcm / 88))
            Gf = 0.073 * Fcm ^ 0.18
            Gch = Gf * (Fcm / Ftm) ^ 2
            Wc = 5.14 * Gf / Ftm
            Deftm = Ftm / E0
            Fc0 = 0.4 * Fcm
            Ft0 = Ftm
            Dim ac, at, bc, bt, DefT As Double
            ac = 7.873
            at = 1
            bc = (1.97 * Fcm / Gch) * Leq
            bt = (0.453 * Fck ^ (2 / 3) / Gf) * Leq


            ' Dc = 1 / (2 + ac)
            '  Dc = Dc * (2 * (1 + ac) * Exp(-bc * DefPlasC) - ac * Exp(-2 * bc * DefPlasC))
            '  Dc = 1 - Dc
            ' SigmaC = Fc0 * ((1 + ac) * Exp(-bc * DefPlasC) - ac * Exp(-2 * bc * DefPlasC))

            E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX, .Y = 850 * EcheY},
                           New PointF With {.X = 50 * EcheX + (Ft0 / E0) * XEc, .Y = 850 * EcheY - Ft0 * YEc})
            Do Until Dt > 0.999
                Id = Id + 5

                DefPlasT = 0.00000001 * Id
                DefT = DefPlasT + Ft0 / E0
                SigmaT = SigmaTII(DefT, Deftm, Leq, Wc, Ftm)


                Dt = 1 - (1 / (2 + at)) * (2 * (1 + at) * Exp(-bt * DefPlasT) - at * Exp(-2 * bt * DefPlasT))

                If OldDefPlasT <> 0 And OldSigmaT <> 0 Then
                    E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX + OldDefPlasT * XEc, .Y = 850 * EcheY - OldSigmaT * YEc},
                           New PointF With {.X = 50 * EcheX + DefT * XEc, .Y = 850 * EcheY - SigmaT * YEc})
                End If
                '     MsgBox(DefPlasT & "    SigmaC=  " & SigmaT)
                OldDefPlasT = DefT
                OldSigmaT = SigmaT
            Loop
        Next


        PenGrid.Width = 1
        PenGrid.Color = Color.Red

        'Titre de courbe
        '      E.FillRectangle(New SolidBrush(Color.White), New Drawing.Rectangle With {.Height = 20, .Width = 175, .Location = New Drawing.Point With {.X = 300 * EcheX - 5, .Y = -9}})
        '     E.DrawRectangle(PenGrid, New Drawing.Rectangle With {.Height = 20, .Width = 175, .Location = New Drawing.Point With {.X = 300 * EcheX - 5, .Y = -9}})
        '    Text = "CDP Model Curve"
        '    E.DrawString(Text, New Font("Arial", 10), New SolidBrush(Color.Black), New PointF With {.X = 350 * EcheX, .Y = -7}) '+ PhSize.Height / 2
        '    PenGrid.Width =WidthPen
    End Sub
    Public Sub DrawTensileDamageParameterAlfarah(ByRef E As Graphics, W As Single, H As Single, Xmax As Double, Ymax As Double, IndexElement As Integer, WidthPen As Integer)
        If Xmax = 0 Or Ymax = 0 Then Exit Sub
        Dim DrawSize As Size = New Size With {.Height = 1000, .Width = 1000}
        Dim EcheX As Single = W / (DrawSize.Width)
        Dim EcheY As Single = H / (DrawSize.Height)
        Dim Dy As Single = 0.05 * DrawSize.Height * EcheY
        Dim Dx As Single = 0.05 * DrawSize.Width * EcheX
        Dim PenGrid As New Pen(Color.Gray)
        Dim PenGraph As New Pen(Color.Blue)
        Dim ELE As BrickEightNodes
        If IndexElement <= Elements.Count - 1 Then
            ELE = Elements.Item(IndexElement)
        Else
            MsgBox("Impossible de trouver cet élément")
            Exit Sub
        End If
        E.TranslateTransform(Dx, Dy)
        Dim Font As New Font("Arial", 7)

        Dim Text As String
        Dim PhSize As SizeF

        Dim XEc As Double = 850 * EcheX / Xmax
        Dim YEc As Double = 850 * EcheY / Ymax

        'tracer les axes
        PenGrid.Width = WidthPen
        PenGrid.Color = Color.Gray
        E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX, .Y = 0}, New PointF With {.X = 50 * EcheX, .Y = 850 * EcheY})
        E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX, .Y = 850 * EcheY}, New PointF With {.X = 900 * EcheX, .Y = 850 * EcheY})

        PenGrid.Width = 1



        For i = 1 To 10
            E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX, .Y = 850 * EcheY - (i * Ymax / 10) * YEc},
                       New PointF With {.X = 900 * EcheX, .Y = 850 * EcheY - (i * Ymax / 10) * YEc})
            E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX + (i * Xmax / 10) * XEc, .Y = 0},
              New PointF With {.X = 50 * EcheX + (i * Xmax / 10) * XEc, .Y = 850 * EcheY})

            Text = RendreString(i * Ymax / 10, 1)
            PhSize = E.MeasureString(Text, Font)
            E.DrawString(Text, Font, New SolidBrush(Color.Black),
                         New PointF With {.X = 50 * EcheX - PhSize.Width - 3, .Y = 850 * EcheY - (i * Ymax / 10) * YEc - PhSize.Height / 2}) '+ PhSize.Height / 2

            Text = RendreString(i * Xmax / 10, 4)
            PhSize = E.MeasureString(Text, Font)
            E.DrawString(Text, Font, New SolidBrush(Color.Black),
                         New PointF With {.X = 50 * EcheX + (i * Xmax / 10) * XEc - PhSize.Width / 2, .Y = 850 * EcheY + PhSize.Height + 3}) '+ PhSize.Height / 2



        Next
        PenGrid.Width = WidthPen
        PenGrid.Color = Color.Blue
        PenGrid.DashStyle = Drawing2D.DashStyle.Solid
        Dim Fck As Double = ELE.Material.Fck
        Dim Leq As Double = ELE.Material.Leq
        Dim Hc, Dt, DefPlasT, OldDefPlasT, SigmaT, OldSigmaT As Double

        ' DefPlasC = 0.000005 * i

        Dim LeqMtr(4) As Double
        LeqMtr(1) = 50
        LeqMtr(2) = 100
        LeqMtr(3) = 200
        LeqMtr(4) = 400
        Dim OurColor(4) As Color
        OurColor(1) = Color.Blue
        OurColor(2) = Color.DarkBlue
        OurColor(3) = Color.Red
        OurColor(4) = Color.DarkRed
        For jj = 1 To 4
            Dt = 0
            Leq = LeqMtr(jj)
            PenGrid.Color = OurColor(jj)
            OldDefPlasT = 0
            OldSigmaT = 0

            Dim Fcm, Fc0, Ft0, Ftm, Defcm, Deftm, Eci, E0, Gf, Gch, Wc, Id As Double
            Id = 0
            Fcm = Fck + 8
            Ftm = 0.3016 * Fck ^ (2 / 3)
            Defcm = 0.0007 * Fcm ^ 0.31
            Eci = 10000 * Fcm ^ (1 / 3)
            E0 = Eci * (0.8 + 0.2 * (Fcm / 88))
            Gf = 0.073 * Fcm ^ 0.18
            Gch = Gf * (Fcm / Ftm) ^ 2
            Wc = 5.14 * Gf / Ftm
            Deftm = Ftm / E0
            Fc0 = 0.4 * Fcm
            Ft0 = Ftm
            Dim ac, at, bc, bt, DefT As Double
            ac = 7.873
            at = 1
            bc = (1.97 * Fcm / Gch) * Leq
            bt = (0.453 * Fck ^ (2 / 3) / Gf) * Leq


            ' Dc = 1 / (2 + ac)
            '  Dc = Dc * (2 * (1 + ac) * Exp(-bc * DefPlasC) - ac * Exp(-2 * bc * DefPlasC))
            '  Dc = 1 - Dc
            ' SigmaC = Fc0 * ((1 + ac) * Exp(-bc * DefPlasC) - ac * Exp(-2 * bc * DefPlasC))

            Do Until Dt > 0.99999
                Id = Id + 500

                DefPlasT = 0.00000001 * Id
                DefT = DefPlasT + Ft0 / E0
                SigmaT = SigmaTII(DefT, Deftm, Leq, Wc, Ftm)


                Dt = 1 - (1 / (2 + at)) * (2 * (1 + at) * Exp(-bt * DefPlasT) - at * Exp(-2 * bt * DefPlasT))

                If OldDefPlasT <> 0 And OldSigmaT <> 0 Then
                    E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX + OldDefPlasT * XEc, .Y = 850 * EcheY - OldSigmaT * YEc},
                           New PointF With {.X = 50 * EcheX + DefT * XEc, .Y = 850 * EcheY - Dt * YEc})
                End If
                '     MsgBox(DefPlasT & "    SigmaC=  " & SigmaT)
                OldDefPlasT = DefT
                OldSigmaT = Dt
            Loop
        Next


        PenGrid.Width = 1
        PenGrid.Color = Color.Red

        'Titre de courbe
        ' E.FillRectangle(New SolidBrush(Color.White), New Drawing.Rectangle With {.Height = 20, .Width = 175, .Location = New Drawing.Point With {.X = 300 * EcheX - 5, .Y = -9}})
        'E.DrawRectangle(PenGrid, New Drawing.Rectangle With {.Height = 20, .Width = 175, .Location = New Drawing.Point With {.X = 300 * EcheX - 5, .Y = -9}})
        'Text = "CDP Model Curve"
        'E.DrawString(Text, New Font("Arial", 10), New SolidBrush(Color.Black), New PointF With {.X = 350 * EcheX, .Y = -7}) '+ PhSize.Height / 2
        'PenGrid.Width =WidthPen
    End Sub

    Public Sub DrawTensileCDPMCurveBakhti(ByRef E As Graphics, W As Single, H As Single, Xmax As Double, Ymax As Double, IndexElement As Integer, WidthPen As Integer)
        If Xmax = 0 Or Ymax = 0 Then Exit Sub
        Dim DrawSize As Size = New Size With {.Height = 1000, .Width = 1000}
        Dim EcheX As Single = W / (DrawSize.Width)
        Dim EcheY As Single = H / (DrawSize.Height)
        Dim Dy As Single = 0.05 * DrawSize.Height * EcheY
        Dim Dx As Single = 0.05 * DrawSize.Width * EcheX
        Dim PenGrid As New Pen(Color.Gray)
        Dim PenGraph As New Pen(Color.Blue)
        Dim ELE As BrickEightNodes
        If IndexElement <= Elements.Count - 1 Then
            ELE = Elements.Item(IndexElement)
        Else
            MsgBox("Impossible de trouver cet élément")
            Exit Sub
        End If
        E.TranslateTransform(Dx, Dy)
        Dim Font As New Font("Arial", 7)

        Dim Text As String
        Dim PhSize As SizeF

        Dim XEc As Double = 850 * EcheX / Xmax
        Dim YEc As Double = 850 * EcheY / Ymax

        'tracer les axes
        PenGrid.Width = WidthPen
        PenGrid.Color = Color.Gray
        E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX, .Y = 0}, New PointF With {.X = 50 * EcheX, .Y = 850 * EcheY})
        E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX, .Y = 850 * EcheY}, New PointF With {.X = 900 * EcheX, .Y = 850 * EcheY})

        PenGrid.Width = 1



        For i = 1 To 10
            E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX, .Y = 850 * EcheY - (i * Ymax / 10) * YEc},
                       New PointF With {.X = 900 * EcheX, .Y = 850 * EcheY - (i * Ymax / 10) * YEc})
            E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX + (i * Xmax / 10) * XEc, .Y = 0},
              New PointF With {.X = 50 * EcheX + (i * Xmax / 10) * XEc, .Y = 850 * EcheY})

            Text = RendreString(i * Ymax / 10, 1)
            PhSize = E.MeasureString(Text, Font)
            E.DrawString(Text, Font, New SolidBrush(Color.Black),
                         New PointF With {.X = 50 * EcheX - PhSize.Width - 3, .Y = 850 * EcheY - (i * Ymax / 10) * YEc - PhSize.Height / 2}) '+ PhSize.Height / 2

            Text = RendreString(i * Xmax / 10, 4)
            PhSize = E.MeasureString(Text, Font)
            E.DrawString(Text, Font, New SolidBrush(Color.Black),
                         New PointF With {.X = 50 * EcheX + (i * Xmax / 10) * XEc - PhSize.Width / 2, .Y = 850 * EcheY + PhSize.Height + 3}) '+ PhSize.Height / 2



        Next
        PenGrid.Width = WidthPen
        PenGrid.Color = Color.Red
        PenGrid.DashStyle = Drawing2D.DashStyle.Solid
        Dim FckMtr(4), Ieq As Double
        FckMtr(1) = 200
        FckMtr(2) = 300
        FckMtr(3) = 700
        FckMtr(4) = 50
        Dim OurColor(4) As Color
        OurColor(1) = Color.Blue
        OurColor(2) = Color.DarkBlue
        OurColor(3) = Color.Red
        OurColor(4) = Color.DarkRed
        For jj = 1 To 1
            Dim Fck As Double = ELE.Material.Fck 'Por Ftm = 3.48 --> Fck = 39.195
            Dim Dt, DefPlasT, OldDefPlasT, SigmaT, OldSigmaT, TotalStrain As Double
            PenGrid.Color = OurColor(jj)



            Dim Fcm, Fc0, Ft0, Ftm, Defcm, Deftm, Eci, E0, Gf, Gch, Wc As Double
            Fcm = Fck + 8
            Ftm = 0.3016 * Fck ^ (2 / 3)
            Defcm = 0.0007 * Fcm ^ 0.31
            '  Ieq = FckMtr(jj)

            Eci = 10000 * Fcm ^ (1 / 3)
            E0 = Eci * (0.8 + 0.2 * (Fcm / 88))
            Gf = 0.073 * Fcm ^ 0.18
            Gch = Gf * (Fcm / Ftm) ^ 2
            Wc = 5.14 * Gf / Ftm
            Deftm = Ftm / E0
            Fc0 = 0.4 * Fcm
            Ft0 = Ftm
            Dim ac, at, bc, bt As Double

            ac = 7.873
            at = 1
            bc = ComputeBcAndBt(Fck, 0.001, bt) ' (1.97 * Fcm / Gch) * Ieq ' (1.97 * Fcm / Gch) * Ieq '  ComputeBcAndBt(Fck, 0.001, bt) ' (1.97 * Fcm / Gch) * Ieq '  
            'bt = (0.453 * Fck ^ (2 / 3) / Gf) * Ieq ' ELE.BtBakhti ' 
            Dim id As Single
            Dt = 0
            E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX, .Y = 850 * EcheY},
                                 New PointF With {.X = 50 * EcheX + (Ft0 / E0) * XEc, .Y = 850 * EcheY - Ft0 * YEc})
            id = 0
            Do Until Dt > 0.98


                Dt = 1 / (2 + at)
                DefPlasT = 0.000001 * id
                Dt = 1 - (Dt * (2 * (1 + at) * Exp(-bt * DefPlasT) - at * Exp(-2 * bt * DefPlasT)))
                SigmaT = Ft0 * ((1 + at) * Exp(-bt * DefPlasT) - at * Exp(-2 * bt * DefPlasT))
                TotalStrain = DefPlasT + Ft0 / E0



                If DefPlasT <> 0 And OldSigmaT <> 0 Then
                    E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX + OldDefPlasT * XEc, .Y = 850 * EcheY - OldSigmaT * YEc},
                           New PointF With {.X = 50 * EcheX + TotalStrain * XEc, .Y = 850 * EcheY - SigmaT * YEc})
                End If

                OldDefPlasT = TotalStrain
                OldSigmaT = SigmaT
                'MsgBox(DefPlasT & "  SigmaT=  " & SigmaT)
                id += 25
            Loop

        Next

        PenGrid.Width = 1
        PenGrid.Color = Color.Red

        'Titre de courbe
        '    E.FillRectangle(New SolidBrush(Color.White), New Drawing.Rectangle With {.Height = 20, .Width = 175, .Location = New Drawing.Point With {.X = 300 * EcheX - 5, .Y = -9}})
        '    E.DrawRectangle(PenGrid, New Drawing.Rectangle With {.Height = 20, .Width = 175, .Location = New Drawing.Point With {.X = 300 * EcheX - 5, .Y = -9}})
        '    Text = "CDP Model Curve"
        '    E.DrawString(Text, New Font("Arial", 10), New SolidBrush(Color.Black), New PointF With {.X = 350 * EcheX, .Y = -7}) '+ PhSize.Height / 2
        PenGrid.Width = WidthPen
    End Sub
    Function ComputeBcAndBt(Fck As Double, Tol As Double, ByRef bt As Double) As Double
        Dim Fcm, Fc0, Ftm, Defc1, Eci, E0, Ft0, Gf, Gch As Double
        Dim SigmaC, OldDefSigmaC, bcStep, ac, at, bc As Double
        Fcm = Fck + 8
        Ftm = 0.3016 * Fck ^ (2 / 3)
        Eci = 10000 * Fcm ^ (1 / 3)
        E0 = Eci * (0.8 + 0.2 * (Fcm / 88))
        Gf = 0.073 * Fcm ^ 0.18
        Gch = Gf * (Fcm / Ftm) ^ 2
        Fc0 = 0.4 * Fcm
        ac = 7.873
        at = 1
        Defc1 = 0.0007 * Fcm ^ 0.31
        If Defc1 > 0.0028 Then Defc1 = 0.0028
        Defc1 -= Fcm / E0
        Ft0 = 0.3016 * Fck ^ (2 / 3)
        bcStep = 10
        Do
            bc += bcStep
            SigmaC = Fc0 * ((1 + ac) * Exp(-bc * Defc1) - ac * Exp(-2 * bc * Defc1))
            If SigmaC > OldDefSigmaC Then
                OldDefSigmaC = SigmaC
            Else
                bc -= 2 * bcStep
                OldDefSigmaC = Fc0 * ((1 + ac) * Exp(-bc * Defc1) - ac * Exp(-2 * bc * Defc1))
                bcStep = bcStep / 10
            End If
        Loop Until bcStep < Tol
        bt = bc * (Ft0 / Fc0) * (Gch / Gf) * ((1 + 0.5 * at) / (1 + 0.5 * ac))
        Return bc
    End Function
    Public Sub DrawTensileDamageParameterBakhti(ByRef E As Graphics, W As Single, H As Single, Xmax As Double, Ymax As Double, IndexElement As Integer, WidthPen As Integer)
        If Xmax = 0 Or Ymax = 0 Then Exit Sub
        Dim DrawSize As Size = New Size With {.Height = 1000, .Width = 1000}
        Dim EcheX As Single = W / (DrawSize.Width)
        Dim EcheY As Single = H / (DrawSize.Height)
        Dim Dy As Single = 0.05 * DrawSize.Height * EcheY
        Dim Dx As Single = 0.05 * DrawSize.Width * EcheX
        Dim PenGrid As New Pen(Color.Gray)
        Dim PenGraph As New Pen(Color.Blue)
        Dim ELE As BrickEightNodes
        If IndexElement <= Elements.Count - 1 Then
            ELE = Elements.Item(IndexElement)
        Else
            MsgBox("Impossible de trouver cet élément")
            Exit Sub
        End If
        E.TranslateTransform(Dx, Dy)
        Dim Font As New Font("Arial", 7)

        Dim Text As String
        Dim PhSize As SizeF

        Dim XEc As Double = 850 * EcheX / Xmax
        Dim YEc As Double = 850 * EcheY / Ymax

        'tracer les axes
        PenGrid.Width = WidthPen
        PenGrid.Color = Color.Gray
        E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX, .Y = 0}, New PointF With {.X = 50 * EcheX, .Y = 850 * EcheY})
        E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX, .Y = 850 * EcheY}, New PointF With {.X = 900 * EcheX, .Y = 850 * EcheY})

        PenGrid.Width = 1

        For i = 1 To 10
            E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX, .Y = 850 * EcheY - (i * Ymax / 10) * YEc},
                       New PointF With {.X = 900 * EcheX, .Y = 850 * EcheY - (i * Ymax / 10) * YEc})
            E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX + (i * Xmax / 10) * XEc, .Y = 0},
              New PointF With {.X = 50 * EcheX + (i * Xmax / 10) * XEc, .Y = 850 * EcheY})

            Text = RendreString(i * Ymax / 10, 1)
            PhSize = E.MeasureString(Text, Font)
            E.DrawString(Text, Font, New SolidBrush(Color.Black),
                         New PointF With {.X = 50 * EcheX - PhSize.Width - 3, .Y = 850 * EcheY - (i * Ymax / 10) * YEc - PhSize.Height / 2}) '+ PhSize.Height / 2

            Text = RendreString(i * Xmax / 10, 4)
            PhSize = E.MeasureString(Text, Font)
            E.DrawString(Text, Font, New SolidBrush(Color.Black),
                         New PointF With {.X = 50 * EcheX + (i * Xmax / 10) * XEc - PhSize.Width / 2, .Y = 850 * EcheY + PhSize.Height + 3}) '+ PhSize.Height / 2



        Next
        PenGrid.Width = WidthPen

        PenGrid.DashStyle = Drawing2D.DashStyle.Solid


        Dim FckMtr(4) As Double
        FckMtr(1) = 20
        FckMtr(2) = 30
        FckMtr(3) = 40
        FckMtr(4) = 50
        Dim OurColor(4) As Color
        OurColor(1) = Color.Blue
        OurColor(2) = Color.DarkBlue
        OurColor(3) = Color.Red
        OurColor(4) = Color.DarkRed
        For jj = 1 To 4
            PenGrid.Color = OurColor(jj)

            Dim Fck As Double = FckMtr(jj) 'ELE.Fck

            Dim Dt, DefPlasT, OldDefPlasT, SigmaT, OldSigmaT, TotalStrain As Double




            Dim Fcm, Fc0, Ft0, Ftm, Defcm, Deftm, Eci, E0, Gf, Gch, Wc As Double
            Fcm = Fck + 8
            Ftm = 0.3016 * Fck ^ (2 / 3)
            Defcm = 0.0007 * Fcm ^ 0.31


            Eci = 10000 * Fcm ^ (1 / 3)
            E0 = Eci * (0.8 + 0.2 * (Fcm / 88))
            Gf = 0.073 * Fcm ^ 0.18
            Gch = Gf * (Fcm / Ftm) ^ 2
            Wc = 5.14 * Gf / Ftm
            Deftm = Ftm / E0
            Fc0 = 0.4 * Fcm
            Ft0 = Ftm
            Dim ac, at, bc, bt As Double
            ac = 7.873
            at = 1

            bc = ComputeBcAndBt(Fck, 0.001, bt) '  bc = ELE.BcBakhti 
            ' bt = ELE.BtBakhti 
            Dim id As Single
            id = 0
            Dt = 0
            OldDefPlasT = 0
            OldSigmaT = 0
            Do Until Dt > 0.999
                Dt = 1 / (2 + at)
                DefPlasT = 0.000001 * id
                Dt = 1 - (Dt * (2 * (1 + at) * Exp(-bt * DefPlasT) - at * Exp(-2 * bt * DefPlasT)))
                SigmaT = Ft0 * ((1 + at) * Exp(-bt * DefPlasT) - at * Exp(-2 * bt * DefPlasT))
                TotalStrain = DefPlasT + Ft0 / E0

                If OldDefPlasT <> 0 And OldSigmaT <> 0 Then
                    E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX + OldDefPlasT * XEc, .Y = 850 * EcheY - OldSigmaT * YEc},
                               New PointF With {.X = 50 * EcheX + TotalStrain * XEc, .Y = 850 * EcheY - Dt * YEc})
                End If
                '     MsgBox(DefPlasT & "    SigmaC=  " & SigmaT)
                OldDefPlasT = TotalStrain
                OldSigmaT = Dt

                'MsgBox(DefPlasT & "  SigmaT=  " & SigmaT)
                id += 1
            Loop

        Next

        PenGrid.Width = 1
        PenGrid.Color = Color.Red

        'Titre de courbe
        '    E.FillRectangle(New SolidBrush(Color.White), New Drawing.Rectangle With {.Height = 20, .Width = 175, .Location = New Drawing.Point With {.X = 300 * EcheX - 5, .Y = -9}})
        '    E.DrawRectangle(PenGrid, New Drawing.Rectangle With {.Height = 20, .Width = 175, .Location = New Drawing.Point With {.X = 300 * EcheX - 5, .Y = -9}})
        '    Text = "CDP Model Curve"
        '    E.DrawString(Text, New Font("Arial", 10), New SolidBrush(Color.Black), New PointF With {.X = 350 * EcheX, .Y = -7}) '+ PhSize.Height / 2
        PenGrid.Width = WidthPen
    End Sub
    Public Sub DrawComprissiveCDPMCurveBakhti(ByRef E As Graphics, W As Single, H As Single, Xmax As Double, Ymax As Double, IndexElement As Integer, WidthPen As Integer)
        If Xmax = 0 Or Ymax = 0 Then Exit Sub
        Dim DrawSize As Size = New Size With {.Height = 1000, .Width = 1000}
        Dim EcheX As Single = W / (DrawSize.Width)
        Dim EcheY As Single = H / (DrawSize.Height)
        Dim Dy As Single = 0.05 * DrawSize.Height * EcheY
        Dim Dx As Single = 0.05 * DrawSize.Width * EcheX
        Dim PenGrid As New Pen(Color.Gray)
        Dim PenGraph As New Pen(Color.Blue)
        Dim ELE As BrickEightNodes
        If IndexElement <= Elements.Count - 1 Then
            ELE = Elements.Item(IndexElement)
        Else
            MsgBox("Impossible de trouver cet élément")
            Exit Sub
        End If
        E.TranslateTransform(Dx, Dy)
        Dim Font As New Font("Arial", 7)

        Dim Text As String
        Dim PhSize As SizeF

        Dim XEc As Double = 850 * EcheX / Xmax
        Dim YEc As Double = 850 * EcheY / Ymax

        'tracer les axes
        PenGrid.Width = WidthPen
        PenGrid.Color = Color.Gray
        E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX, .Y = 0}, New PointF With {.X = 50 * EcheX, .Y = 850 * EcheY})
        E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX, .Y = 850 * EcheY}, New PointF With {.X = 900 * EcheX, .Y = 850 * EcheY})

        PenGrid.Width = 1



        For i = 1 To 10
            E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX, .Y = 850 * EcheY - (i * Ymax / 10) * YEc},
                       New PointF With {.X = 900 * EcheX, .Y = 850 * EcheY - (i * Ymax / 10) * YEc})
            E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX + (i * Xmax / 10) * XEc, .Y = 0},
              New PointF With {.X = 50 * EcheX + (i * Xmax / 10) * XEc, .Y = 850 * EcheY})

            Text = RendreString(i * Ymax / 10, 1)
            PhSize = E.MeasureString(Text, Font)
            E.DrawString(Text, Font, New SolidBrush(Color.Black),
                         New PointF With {.X = 50 * EcheX - PhSize.Width - 3, .Y = 850 * EcheY - (i * Ymax / 10) * YEc - PhSize.Height / 2}) '+ PhSize.Height / 2

            Text = RendreString(i * Xmax / 10, 4)
            PhSize = E.MeasureString(Text, Font)
            E.DrawString(Text, Font, New SolidBrush(Color.Black),
                         New PointF With {.X = 50 * EcheX + (i * Xmax / 10) * XEc - PhSize.Width / 2, .Y = 850 * EcheY + PhSize.Height + 3}) '+ PhSize.Height / 2



        Next
        PenGrid.Width = WidthPen
        PenGrid.Color = Color.Blue
        PenGrid.DashStyle = Drawing2D.DashStyle.Solid
        Dim FckMtr(4), Ieq As Double
        FckMtr(1) = ELE.Material.Fck
        FckMtr(2) = 30
        FckMtr(3) = 40
        FckMtr(4) = 50
        Dim OurColor(4) As Color
        OurColor(1) = Color.Blue
        OurColor(2) = Color.DarkBlue
        OurColor(3) = Color.Red
        OurColor(4) = Color.DarkRed
        For jj = 1 To 1
            Dim Fck As Double = FckMtr(jj) ' ELE.Fck

            Dim Dc, DefPlasC, OldDefPlasC, SigmaC, OldSigmaC, TotalStrain As Double


            ' DefPlasC = 0.000005 * i

            Dc = 0
            '   Ieq = LeqMtr(jj)
            PenGrid.Color = OurColor(jj)
            OldDefPlasC = 0
            OldSigmaC = 0

            Dim Fcm, Fc0, Ft0, Ftm, Defcm, Deftm, Eci, E0, Gf, Gch, Wc As Double
            Fcm = Fck + 8
            Ftm = 0.3016 * Fck ^ (2 / 3)
            Defcm = 0.0007 * Fcm ^ 0.31


            Eci = 10000 * Fcm ^ (1 / 3)
            E0 = Eci * (0.8 + 0.2 * (Fcm / 88))
            Gf = 0.073 * Fcm ^ 0.18
            Gch = Gf * (Fcm / Ftm) ^ 2
            Wc = 5.14 * Gf / Ftm
            Deftm = Ftm / E0
            Fc0 = 0.4 * Fcm
            Ft0 = Ftm
            Dim ac, at, bc, bt As Double
            ac = 7.873
            at = 1

            bc = ComputeBcAndBt(Fck, 0.001, bt) ' (1.97 * Fcm / Gch) * Ieq '  
            'bt = (0.453 * Fck ^ (2 / 3) / Gf) * Ieq ' ELE.BtBakhti ' 
            Dim id As Single
            id = 0
            E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX, .Y = 850 * EcheY},
                           New PointF With {.X = 50 * EcheX + (Fc0 / E0) * XEc, .Y = 850 * EcheY - Fc0 * YEc})

            Do Until Dc > 0.9995
                Dc = 1 / (2 + ac)
                DefPlasC = 0.000005 * id
                Dc = Dc * (2 * (1 + ac) * Exp(-bc * DefPlasC) - ac * Exp(-2 * bc * DefPlasC))
                Dc = 1 - Dc
                SigmaC = Fc0 * ((1 + ac) * Exp(-bc * DefPlasC) - ac * Exp(-2 * bc * DefPlasC))
                TotalStrain = DefPlasC + SigmaC / E0

                If DefPlasC <> 0 And OldSigmaC <> 0 Then
                    E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX + OldDefPlasC * XEc, .Y = 850 * EcheY - OldSigmaC * YEc},
                           New PointF With {.X = 50 * EcheX + TotalStrain * XEc, .Y = 850 * EcheY - SigmaC * YEc})
                End If

                OldDefPlasC = TotalStrain
                OldSigmaC = SigmaC
                id += 10
            Loop
        Next

        PenGrid.Width = 1
        PenGrid.Color = Color.Red

        'Titre de courbe
        '  E.FillRectangle(New SolidBrush(Color.White), New Drawing.Rectangle With {.Height = 20, .Width = 175, .Location = New Drawing.Point With {.X = 300 * EcheX - 5, .Y = -9}})
        'E.DrawRectangle(PenGrid, New Drawing.Rectangle With {.Height = 20, .Width = 175, .Location = New Drawing.Point With {.X = 300 * EcheX - 5, .Y = -9}})
        '     Text = "CDP Model Curve"
        '      E.DrawString(Text, New Font("Arial", 10), New SolidBrush(Color.Black), New PointF With {.X = 350 * EcheX, .Y = -7}) '+ PhSize.Height / 2
        '       PenGrid.Width =WidthPen
        Text = "Fcm= " & (PropretyBeton.Fck + 8).ToString & " MPa, " & "Ftm= " & RendreString(0.3016 * PropretyBeton.Fck ^ (2 / 3), 2) & " MPa, " & "E0= " & RendreString(PropretyBeton.E.ToString, 0) & " MPa."
        PhSize = E.MeasureString(Text, Font)

        E.FillRectangle(New SolidBrush(Color.White), New Drawing.Rectangle With {.Height = 40, .Width = 330, .Location = New Drawing.Point With {.X = 80 * EcheX - 5, .Y = -15}})
        E.DrawRectangle(PenGrid, New Drawing.Rectangle With {.Height = 40, .Width = 330, .Location = New Drawing.Point With {.X = 80 * EcheX - 5, .Y = -15}})


        E.DrawString(Text, New Font("Arial", 10), New SolidBrush(Color.Black), New PointF With {.X = 100 * EcheX, .Y = PhSize.Height - 7}) ' / 2

        Text = "Bakhti Compressive σ-ε --> Element " & (IndexElement + 1).ToString & " / " & OurProblem.Elements.Count
        PhSize = E.MeasureString(Text, Font)
        E.DrawString(Text, New Font("Arial", 10), New SolidBrush(Color.Black), New PointF With {.X = 250 * EcheX, .Y = -15}) '+ PhSize.Height / 2

    End Sub
    Public Sub DrawComprissiveDamageParameterBakhti(ByRef E As Graphics, W As Single, H As Single, Xmax As Double, Ymax As Double, IndexElement As Integer, WidthPen As Integer)
        If Xmax = 0 Or Ymax = 0 Then Exit Sub
        Dim DrawSize As Size = New Size With {.Height = 1000, .Width = 1000}
        Dim EcheX As Single = W / (DrawSize.Width)
        Dim EcheY As Single = H / (DrawSize.Height)
        Dim Dy As Single = 0.05 * DrawSize.Height * EcheY
        Dim Dx As Single = 0.05 * DrawSize.Width * EcheX
        Dim PenGrid As New Pen(Color.Gray)
        Dim PenGraph As New Pen(Color.Blue)
        Dim ELE As BrickEightNodes
        If IndexElement <= Elements.Count - 1 Then
            ELE = Elements.Item(IndexElement)
        Else
            MsgBox("Impossible de trouver cet élément")
            Exit Sub
        End If
        E.TranslateTransform(Dx, Dy)
        Dim Font As New Font("Arial", 7)

        Dim Text As String
        Dim PhSize As SizeF

        Dim XEc As Double = 850 * EcheX / Xmax
        Dim YEc As Double = 850 * EcheY / Ymax

        'tracer les axes
        PenGrid.Width = WidthPen
        PenGrid.Color = Color.Gray
        E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX, .Y = 0}, New PointF With {.X = 50 * EcheX, .Y = 850 * EcheY})
        E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX, .Y = 850 * EcheY}, New PointF With {.X = 900 * EcheX, .Y = 850 * EcheY})

        PenGrid.Width = 1



        For i = 1 To 10
            E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX, .Y = 850 * EcheY - (i * Ymax / 10) * YEc},
                       New PointF With {.X = 900 * EcheX, .Y = 850 * EcheY - (i * Ymax / 10) * YEc})
            E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX + (i * Xmax / 10) * XEc, .Y = 0},
              New PointF With {.X = 50 * EcheX + (i * Xmax / 10) * XEc, .Y = 850 * EcheY})

            Text = RendreString(i * Ymax / 10, 1)
            PhSize = E.MeasureString(Text, Font)
            E.DrawString(Text, Font, New SolidBrush(Color.Black),
                         New PointF With {.X = 50 * EcheX - PhSize.Width - 3, .Y = 850 * EcheY - (i * Ymax / 10) * YEc - PhSize.Height / 2}) '+ PhSize.Height / 2

            Text = RendreString(i * Xmax / 10, 4)
            PhSize = E.MeasureString(Text, Font)
            E.DrawString(Text, Font, New SolidBrush(Color.Black),
                         New PointF With {.X = 50 * EcheX + (i * Xmax / 10) * XEc - PhSize.Width / 2, .Y = 850 * EcheY + PhSize.Height + 3}) '+ PhSize.Height / 2



        Next
        PenGrid.Width = WidthPen
        PenGrid.Color = Color.Blue
        PenGrid.DashStyle = Drawing2D.DashStyle.Solid
        Dim FckMtr(4) As Double
        FckMtr(1) = 20
        FckMtr(2) = 30
        FckMtr(3) = 40
        FckMtr(4) = 50
        Dim OurColor(4) As Color
        OurColor(1) = Color.Blue
        OurColor(2) = Color.DarkBlue
        OurColor(3) = Color.Red
        OurColor(4) = Color.DarkRed
        Dim Ieq As Double
        For jj = 1 To 4
            Dim Fck As Double = FckMtr(jj) ' ELE.Fck

            Dim Dc, DefPlasC, OldDefPlasC, SigmaC, OldSigmaC, TotalStrain As Double


            ' DefPlasC = 0.000005 * i

            Dc = 0
            '     Ieq = LeqMtr(jj)
            PenGrid.Color = OurColor(jj)
            OldDefPlasC = 0
            OldSigmaC = 0

            Dim Fcm, Fc0, Ft0, Ftm, Defcm, Deftm, Eci, E0, Gf, Gch, Wc As Double
            Fcm = Fck + 8
            Ftm = 0.3016 * Fck ^ (2 / 3)
            Defcm = 0.0007 * Fcm ^ 0.31


            Eci = 10000 * Fcm ^ (1 / 3)
            E0 = Eci * (0.8 + 0.2 * (Fcm / 88))
            Gf = 0.073 * Fcm ^ 0.18
            Gch = Gf * (Fcm / Ftm) ^ 2
            Wc = 5.14 * Gf / Ftm
            Deftm = Ftm / E0
            Fc0 = 0.4 * Fcm
            Ft0 = Ftm
            Dim ac, at, bc, bt As Double
            ac = 7.873
            at = 1
            bc = ELE.Material.bc '(1.97 * Fcm / Gch) * Ieq' ComputeBcAndBt(Fck, 0.001, bt) ' 
            bt = (0.453 * Fck ^ (2 / 3) / Gf) * Ieq ' ELE.BtBakhti '
            Dim id As Single
            id = 0

            Do Until Dc > 0.9995
                Dc = 1 / (2 + ac)
                DefPlasC = 0.000005 * id
                Dc = Dc * (2 * (1 + ac) * Exp(-bc * DefPlasC) - ac * Exp(-2 * bc * DefPlasC))
                Dc = 1 - Dc
                SigmaC = Fc0 * ((1 + ac) * Exp(-bc * DefPlasC) - ac * Exp(-2 * bc * DefPlasC))
                TotalStrain = DefPlasC + SigmaC / E0
                MsgBox("Strain = " & DefPlasC & "      damage=" & Dc)
                If OldDefPlasC <> 0 And OldSigmaC <> 0 Then
                    E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX + OldDefPlasC * XEc, .Y = 850 * EcheY - OldSigmaC * YEc},
                           New PointF With {.X = 50 * EcheX + TotalStrain * XEc, .Y = 850 * EcheY - Dc * YEc})
                End If

                OldDefPlasC = TotalStrain
                OldSigmaC = Dc
                id += 50
            Loop

        Next

        PenGrid.Width = 1
        PenGrid.Color = Color.Red

        'Titre de courbe
        '  E.FillRectangle(New SolidBrush(Color.White), New Drawing.Rectangle With {.Height = 20, .Width = 175, .Location = New Drawing.Point With {.X = 300 * EcheX - 5, .Y = -9}})
        'E.DrawRectangle(PenGrid, New Drawing.Rectangle With {.Height = 20, .Width = 175, .Location = New Drawing.Point With {.X = 300 * EcheX - 5, .Y = -9}})
        '     Text = "CDP Model Curve"
        '      E.DrawString(Text, New Font("Arial", 10), New SolidBrush(Color.Black), New PointF With {.X = 350 * EcheX, .Y = -7}) '+ PhSize.Height / 2
        '       PenGrid.Width =WidthPen
    End Sub
    Public Sub DrawComprissiveDamageParameterCurve(ByRef E As Graphics, W As Single, H As Single, Xmax As Double, Ymax As Double, IndexElement As Integer, WidthPen As Integer)
        If Xmax = 0 Or Ymax = 0 Then Exit Sub
        Dim DrawSize As Size = New Size With {.Height = 1000, .Width = 1000}
        Dim EcheX As Single = W / (DrawSize.Width)
        Dim EcheY As Single = H / (DrawSize.Height)
        Dim Dy As Single = 0.05 * DrawSize.Height * EcheY
        Dim Dx As Single = 0.05 * DrawSize.Width * EcheX
        Dim PenGrid As New Pen(Color.Gray)
        Dim PenGraph As New Pen(Color.Blue)
        Dim ELE As BrickEightNodes
        If IndexElement <= Elements.Count - 1 Then
            ELE = Elements.Item(IndexElement)
        Else
            MsgBox("Impossible de trouver cet élément")
            Exit Sub
        End If
        E.TranslateTransform(Dx, Dy)
        Dim Font As New Font("Arial", 7)

        Dim Text As String
        Dim PhSize As SizeF

        Dim XEc As Double = 850 * EcheX / Xmax
        Dim YEc As Double = 850 * EcheY / Ymax

        'tracer les axes
        PenGrid.Width = WidthPen
        PenGrid.Color = Color.Red
        E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX, .Y = 0}, New PointF With {.X = 50 * EcheX, .Y = 850 * EcheY})
        E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX, .Y = 850 * EcheY}, New PointF With {.X = 900 * EcheX, .Y = 850 * EcheY})

        PenGrid.Width = 1
        PenGrid.Color = Color.Red

        PenGrid.DashStyle = Drawing2D.DashStyle.Dash
        For i = 1 To 10
            E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX, .Y = 850 * EcheY - (i * Ymax / 10) * YEc},
                       New PointF With {.X = 900 * EcheX, .Y = 850 * EcheY - (i * Ymax / 10) * YEc})
            E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX + (i * Xmax / 10) * XEc, .Y = 0},
              New PointF With {.X = 50 * EcheX + (i * Xmax / 10) * XEc, .Y = 850 * EcheY})

            Text = RendreString(i * Ymax / 10, 1)
            PhSize = E.MeasureString(Text, Font)
            E.DrawString(Text, Font, New SolidBrush(Color.Black),
                         New PointF With {.X = 50 * EcheX - PhSize.Width - 3, .Y = 850 * EcheY - (i * Ymax / 10) * YEc - PhSize.Height / 2}) '+ PhSize.Height / 2

            Text = RendreString(i * Xmax / 10, 4)
            PhSize = E.MeasureString(Text, Font)
            E.DrawString(Text, Font, New SolidBrush(Color.Black),
                         New PointF With {.X = 50 * EcheX + (i * Xmax / 10) * XEc - PhSize.Width / 2, .Y = 850 * EcheY + PhSize.Height + 3}) '+ PhSize.Height / 2



        Next
        PenGrid.Width = WidthPen
        PenGrid.Color = Color.Blue
        PenGrid.DashStyle = Drawing2D.DashStyle.Solid
        Dim Fck As Double = ELE.Material.Fck

        Dim Hc, Dc, OldDc, DefPlasC, OldDefPlasC As Double
        Dim id As Integer = -10
        Do Until OldDc > 0.999
            id += 10

            DefPlasC = 0.000005 * id
            Hc = 0.9

            Dim Fcm, Fc0, Ft0, Ftm, Defcm, Deftm, Eci, E0, Gf, Gch, Wc As Double
            Fcm = Fck + 8
            Ftm = 0.3016 * Fck ^ (2 / 3)
            Defcm = 0.0007 * Fcm ^ 0.31
            Eci = 10000 * Fcm ^ (1 / 3)
            E0 = Eci * (0.8 + 0.2 * (Fcm / 88))
            Gf = 0.073 * Fcm ^ 0.18
            Gch = Gf * (Fcm / Ftm) ^ 2
            Wc = 5.14 * Gf / Ftm
            Deftm = Ftm / E0
            Fc0 = 0.4 * Fcm
            Ft0 = Ftm
            Dim ac, at, bc, bt As Double
            ac = 7.873
            at = 1
            bc = ELE.Material.bc ' (1.97 * Fcm / Gch) * LeqBakhti
            bt = ELE.Material.bt '(0.453 * Fck ^ (2 / 3) / Gf) * LeqBakhti


            Dc = 1 / (2 + ac)
            Dc = 1 - Dc * (2 * (1 + ac) * Exp(-bc * DefPlasC) - ac * Exp(-2 * bc * DefPlasC))


            If OldDefPlasC <> 0 And OldDc <> 0 Then
                E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX + OldDefPlasC * XEc, .Y = 850 * EcheY - OldDc * YEc},
                       New PointF With {.X = 50 * EcheX + DefPlasC * XEc, .Y = 850 * EcheY - Dc * YEc})
            End If

            OldDefPlasC = DefPlasC
            OldDc = Dc
        Loop



        PenGrid.Width = 1
        PenGrid.Color = Color.Red

        'Titre de courbe
        E.FillRectangle(New SolidBrush(Color.White), New Drawing.Rectangle With {.Height = 20, .Width = 250, .Location = New Drawing.Point With {.X = 200 * EcheX - 5, .Y = -9}})
        E.DrawRectangle(PenGrid, New Drawing.Rectangle With {.Height = 20, .Width = 250, .Location = New Drawing.Point With {.X = 200 * EcheX - 5, .Y = -9}})
        Text = "Comprissive Damage-Parameter Curve"
        E.DrawString(Text, New Font("Arial", 10), New SolidBrush(Color.Black), New PointF With {.X = 200 * EcheX, .Y = -7}) '+ PhSize.Height / 2
        PenGrid.Width = WidthPen
    End Sub
    Public Sub DrawDamageParameterCurve(ByRef E As Graphics, W As Single, H As Single, Xmax As Double, Ymax As Double, IndexElement As Integer, WidthPen As Integer)
        If Xmax = 0 Or Ymax = 0 Then Exit Sub
        Dim DrawSize As Size = New Size With {.Height = 1000, .Width = 1000}
        Dim EcheX As Single = W / (DrawSize.Width)
        Dim EcheY As Single = H / (DrawSize.Height)
        Dim Dy As Single = 0.05 * DrawSize.Height * EcheY
        Dim Dx As Single = 0.05 * DrawSize.Width * EcheX
        Dim PenGrid As New Pen(Color.Gray)
        Dim PenGraph As New Pen(Color.Blue)
        Dim ELE As BrickEightNodes
        If IndexElement <= Elements.Count - 1 Then
            ELE = Elements.Item(IndexElement)
        Else
            MsgBox("Impossible de trouver cet élément")
            Exit Sub
        End If
        E.TranslateTransform(Dx, Dy)
        Dim Font As New Font("Arial", 7)

        Dim Text As String
        Dim PhSize As SizeF

        Dim XEc As Double = 850 * EcheX / Xmax
        Dim YEc As Double = 850 * EcheY / Ymax

        'tracer les axes
        PenGrid.Width = WidthPen
        PenGrid.Color = Color.Red
        E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX, .Y = 0}, New PointF With {.X = 50 * EcheX, .Y = 850 * EcheY})
        E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX, .Y = 850 * EcheY}, New PointF With {.X = 900 * EcheX, .Y = 850 * EcheY})

        PenGrid.Width = 1
        PenGrid.Color = Color.Red

        PenGrid.DashStyle = Drawing2D.DashStyle.Dash
        For i = 1 To 10
            E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX, .Y = 850 * EcheY - (i * Ymax / 10) * YEc},
                       New PointF With {.X = 900 * EcheX, .Y = 850 * EcheY - (i * Ymax / 10) * YEc})
            E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX + (i * Xmax / 10) * XEc, .Y = 0},
              New PointF With {.X = 50 * EcheX + (i * Xmax / 10) * XEc, .Y = 850 * EcheY})

            Text = RendreString(i * Ymax / 10, 1)
            PhSize = E.MeasureString(Text, Font)
            E.DrawString(Text, Font, New SolidBrush(Color.Black),
                         New PointF With {.X = 50 * EcheX - PhSize.Width - 3, .Y = 850 * EcheY - (i * Ymax / 10) * YEc - PhSize.Height / 2}) '+ PhSize.Height / 2

            Text = RendreString(i * Xmax / 10, 4)
            PhSize = E.MeasureString(Text, Font)
            E.DrawString(Text, Font, New SolidBrush(Color.Black),
                         New PointF With {.X = 50 * EcheX + (i * Xmax / 10) * XEc - PhSize.Width / 2, .Y = 850 * EcheY + PhSize.Height + 3}) '+ PhSize.Height / 2



        Next
        PenGrid.Width = WidthPen
        PenGrid.Color = Color.Blue
        PenGrid.DashStyle = Drawing2D.DashStyle.Solid


        If ELE.ListOfDamageParameter.Count >= 2 Then
            For i = 0 To ELE.ListOfDamageParameter.Count - 2
                If i = 0 Then
                    E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX + Abs(ELE.LoadDisplacement.Item(i).x) * XEc, .Y = 850 * EcheY - ELE.ListOfDamageParameter.Item(i) * YEc},
                                      New PointF With {.X = 50 * EcheX, .Y = 850 * EcheY})
                    E.FillRectangle(New SolidBrush(Color.BlueViolet), New Drawing.Rectangle With {.Height = 6, .Width = 6,
                .Location = New Drawing.Point With {.X = 50 * EcheX + Abs(ELE.LoadDisplacement.Item(i).x) * XEc - 3, .Y = 850 * EcheY - ELE.ListOfDamageParameter.Item(i) * YEc - 3}})
                End If
                E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX + Abs(ELE.LoadDisplacement.Item(i).x) * XEc, .Y = 850 * EcheY - ELE.ListOfDamageParameter.Item(i) * YEc},
                       New PointF With {.X = 50 * EcheX + Abs(ELE.LoadDisplacement.Item(i + 1).x) * XEc, .Y = 850 * EcheY - ELE.ListOfDamageParameter.Item(i + 1) * YEc})

                E.FillRectangle(New SolidBrush(Color.BlueViolet), New Drawing.Rectangle With {.Height = 6, .Width = 6,
                            .Location = New Drawing.Point With {.X = 50 * EcheX + Abs(ELE.LoadDisplacement.Item(i + 1).x) * XEc - 3, .Y = 850 * EcheY - ELE.ListOfDamageParameter.Item(i + 1) * YEc - 3}})

            Next
        ElseIf ELE.ListOfDamageParameter.Count = 1 Then
            E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX, .Y = 850 * EcheY},
                       New PointF With {.X = 50 * EcheX + Abs(ELE.LoadDisplacement.Item(0).x) * XEc, .Y = 850 * EcheY - ELE.ListOfDamageParameter.Item(0) * YEc})

            E.FillRectangle(New SolidBrush(Color.BlueViolet), New Drawing.Rectangle With {.Height = 6, .Width = 6,
                            .Location = New Drawing.Point With {.X = 50 * EcheX + Abs(ELE.LoadDisplacement.Item(0).x) * XEc - 3, .Y = 850 * EcheY - ELE.ListOfDamageParameter.Item(0) * YEc - 3}})

        End If





        PenGrid.Width = 1
        PenGrid.Color = Color.Red

        'Titre de courbe
        E.FillRectangle(New SolidBrush(Color.White), New Drawing.Rectangle With {.Height = 20, .Width = 250, .Location = New Drawing.Point With {.X = 200 * EcheX - 5, .Y = -9}})
        E.DrawRectangle(PenGrid, New Drawing.Rectangle With {.Height = 20, .Width = 250, .Location = New Drawing.Point With {.X = 200 * EcheX - 5, .Y = -9}})
        Text = "Displacement Damage-Parameter Curve"
        E.DrawString(Text, New Font("Arial", 10), New SolidBrush(Color.Black), New PointF With {.X = 200 * EcheX, .Y = -7}) '+ PhSize.Height / 2
        PenGrid.Width = WidthPen
    End Sub
    Public Sub DrawLoadDisplacementCurve(ByRef E As Graphics, W As Single, H As Single, Xmax As Double, Ymax As Double, IndexElement As Integer, WidthPen As Integer)
        If Xmax = 0 Or Ymax = 0 Then Exit Sub
        Dim DrawSize As Size = New Size With {.Height = 1000, .Width = 1000}
        Dim EcheX As Single = W / (DrawSize.Width)
        Dim EcheY As Single = H / (DrawSize.Height)
        Dim Dy As Single = 0.05 * DrawSize.Height * EcheY
        Dim Dx As Single = 0.05 * DrawSize.Width * EcheX
        Dim PenGrid As New Pen(Color.Gray)
        Dim PenGraph As New Pen(Color.Blue)
        Dim ELE As BrickEightNodes
        If IndexElement <= Elements.Count - 1 Then
            ELE = Elements.Item(IndexElement)
        Else
            MsgBox("Impossible de trouver cet élément")
            Exit Sub
        End If
        E.TranslateTransform(Dx, Dy)
        Dim Font As New Font("Arial", 7)

        Dim Text As String
        Dim PhSize As SizeF

        Dim XEc As Double = 850 * EcheX / Xmax
        Dim YEc As Double = 850 * EcheY / Ymax

        'tracer les axes
        PenGrid.Width = WidthPen
        PenGrid.Color = Color.Red
        E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX, .Y = 0}, New PointF With {.X = 50 * EcheX, .Y = 850 * EcheY})
        E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX, .Y = 850 * EcheY}, New PointF With {.X = 900 * EcheX, .Y = 850 * EcheY})

        PenGrid.Width = 1
        PenGrid.Color = Color.Red

        PenGrid.DashStyle = Drawing2D.DashStyle.Dash
        For i = 1 To 10
            E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX, .Y = 850 * EcheY - (i * Ymax / 10) * YEc},
                       New PointF With {.X = 900 * EcheX, .Y = 850 * EcheY - (i * Ymax / 10) * YEc})
            E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX + (i * Xmax / 10) * XEc, .Y = 0},
              New PointF With {.X = 50 * EcheX + (i * Xmax / 10) * XEc, .Y = 850 * EcheY})

            Text = RendreString(i * Ymax / 10, 1)
            PhSize = E.MeasureString(Text, Font)
            E.DrawString(Text, Font, New SolidBrush(Color.Black),
                         New PointF With {.X = 50 * EcheX - PhSize.Width - 3, .Y = 850 * EcheY - (i * Ymax / 10) * YEc - PhSize.Height / 2}) '+ PhSize.Height / 2

            Text = RendreString(i * Xmax / 10, 4)
            PhSize = E.MeasureString(Text, Font)
            E.DrawString(Text, Font, New SolidBrush(Color.Black),
                         New PointF With {.X = 50 * EcheX + (i * Xmax / 10) * XEc - PhSize.Width / 2, .Y = 850 * EcheY + PhSize.Height + 3}) '+ PhSize.Height / 2



        Next
        PenGrid.Width = WidthPen
        PenGrid.Color = Color.Blue
        PenGrid.DashStyle = Drawing2D.DashStyle.Solid
        Dim Damage, DamagePlase As Double
        If ELE.LoadDisplacement.Count >= 2 Then
            For i = 0 To ELE.LoadDisplacement.Count - 2
                Damage = ELE.ListOfDamageParameter.Item(i)
                DamagePlase = ELE.ListOfDamageParameter.Item(i + 1)
                'If ELE.LoadDisplacement.Item(i).y <= (ELE.Fck + 8) Then Damage = 0
                'If ELE.LoadDisplacement.Item(i + 1).y <= (ELE.Fck + 8) Then DamagePlase = 0
                If i = 0 Then
                    E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX + Abs(ELE.LoadDisplacement.Item(i).x) * XEc, .Y = 850 * EcheY - ELE.LoadDisplacement.Item(i).y * (1 - Damage) * YEc},
                                        New PointF With {.X = 50 * EcheX, .Y = 850 * EcheY})
                    'E.FillRectangle(New SolidBrush(Color.BlueViolet), New Drawing.Rectangle With {.Height = 6, .Width = 6,
                    '.Location = New Drawing.Point With {.X = 50 * EcheX + Abs(ELE.LoadDisplacement.Item(i).x) * XEc - 3, .Y = 850 * EcheY - ELE.LoadDisplacement.Item(i).y * (1 - Damage) * YEc - 3}})
                End If
                E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX + Abs(ELE.LoadDisplacement.Item(i).x) * XEc, .Y = 850 * EcheY - ELE.LoadDisplacement.Item(i).y * (1 - Damage) * YEc},
                                    New PointF With {.X = 50 * EcheX + Abs(ELE.LoadDisplacement.Item(i + 1).x) * XEc, .Y = 850 * EcheY - ELE.LoadDisplacement.Item(i + 1).y * (1 - DamagePlase) * YEc})
                'E.FillRectangle(New SolidBrush(Color.BlueViolet), New Drawing.Rectangle With {.Height = 6, .Width = 6,
                '.Location = New Drawing.Point With {.X = 50 * EcheX + Abs(ELE.LoadDisplacement.Item(i + 1).x) * XEc - 3, .Y = 850 * EcheY - ELE.LoadDisplacement.Item(i + 1).y * (1 - DamagePlase) * YEc - 3}})

            Next
        ElseIf ELE.LoadDisplacement.Count = 1 Then
            E.DrawLine(PenGrid, New PointF With {.X = 50 * EcheX, .Y = 850 * EcheY},
                                New PointF With {.X = 50 * EcheX + Abs(ELE.LoadDisplacement.Item(0).x) * XEc, .Y = 850 * EcheY - ELE.LoadDisplacement.Item(0).y * (1 - ELE.ListOfDamageParameter.Item(0)) * YEc})

            'E.FillRectangle(New SolidBrush(Color.BlueViolet), New Drawing.Rectangle With {.Height = 6, .Width = 6,
            '.Location = New Drawing.Point With {.X = 50 * EcheX + Abs(ELE.LoadDisplacement.Item(0).x) * XEc - 3, .Y = 850 * EcheY - ELE.LoadDisplacement.Item(0).y * (1 - ELE.ListOfDamageParameter.Item(0)) * YEc - 3}})

        End If
        PenGrid.Width = 1
        PenGrid.Color = Color.Red

        'Titre de courbe
        E.FillRectangle(New SolidBrush(Color.White), New Drawing.Rectangle With {.Height = 20, .Width = 175, .Location = New Drawing.Point With {.X = 300 * EcheX - 5, .Y = -9}})
        E.DrawRectangle(PenGrid, New Drawing.Rectangle With {.Height = 20, .Width = 175, .Location = New Drawing.Point With {.X = 300 * EcheX - 5, .Y = -9}})
        Text = "Loads-Displacements Curve"
        E.DrawString(Text, New Font("Arial", 10), New SolidBrush(Color.Black), New PointF With {.X = 300 * EcheX, .Y = -7}) '+ PhSize.Height / 2
        PenGrid.Width = WidthPen
    End Sub




    Private Sub Assemblage(ByRef TotalVector() As Double, Vector() As Double, Element As BrickEightNodes)
        Dim jp, NI As Single
        Dim ii As Integer
        For ip = 1 To 24
            jp = ip - 3 * Int(ip / 3)
            If jp = 0 Then jp = 3
            NI = Int(ip / 3)
            If NI <> ip / 3 Then NI += 1
            ii = 3 * Element.ListNoeud(NI - 1).Ident + jp - 1
            ii = IndexNoued.Item(ii)
            If ii <> -1 Then
                TotalVector(ii) += Vector(ip)
            End If
        Next
    End Sub
    Public Function CalculeRigiditeInverse() As Double(,)

        ReDim KGlob(Nq, Nq)
        Call Gauss(Ngp)




        ' FORMULATION DE LA MATRICE DE RIGIDITE GLOBALE
        For i = 0 To Elements.Count - 1
            ' FORMULATION DE LA MATRICE DE RIGIDITE ELEMENTAIRE
            Dim Ke(,) As Double = Elements.Item(i).MtriceRigiditeElementaire(Ngp, IndexNoued, samp)
            ' Assemblage
            Dim jp, jq, NI, NJ As Single
            Dim ii, jj As Integer
            For ip = 1 To 24
                For iq = 1 To 24
                    jp = ip - 3 * Int(ip / 3)
                    jq = iq - 3 * Int(iq / 3)
                    If jp = 0 Then jp = 3
                    If jq = 0 Then jq = 3
                    NI = Int(ip / 3)
                    If NI <> ip / 3 Then NI += 1
                    NJ = Int(iq / 3)
                    If NJ <> iq / 3 Then NJ += 1
                    ii = 3 * Elements.Item(i).ListNoeud(NI - 1).Ident + jp - 1
                    jj = 3 * Elements.Item(i).ListNoeud(NJ - 1).Ident + jq - 1
                    ii = IndexNoued.Item(ii)
                    jj = IndexNoued.Item(jj)
                    If ii <> -1 And jj <> -1 Then
                        KGlob(ii, jj) += Elements.Item(i).Ke(ip, iq)
                    End If
                Next
            Next
        Next


        Dim Solve As New MathNet.Numerics.LinearAlgebra.Double.DenseMatrix(Nq + 1)
        For i = 0 To Nq
            For j = 0 To Nq
                Solve(i, j) = KGlob(i, j)
            Next
        Next


        Dim Resulat As New MathNet.Numerics.LinearAlgebra.Double.DenseMatrix(Nq + 1)
        Resulat = Solve.Inverse
        Dim KGlobInver(Nq, Nq) As Double
        For i = 0 To Nq
            For j = 0 To Nq
                KGlobInver(i, j) = Resulat(i, j)
            Next
        Next
        Return KGlobInver

    End Function
    Private Sub TrouverDeplacements()

        Dim Solve As New MathNet.Numerics.LinearAlgebra.Double.DenseMatrix(Nq + 1)
        For i = 0 To Nq
            For j = 0 To Nq
                Solve(i, j) = KGlob(i, j)
            Next
        Next
        Dim MesForces As New MathNet.Numerics.LinearAlgebra.Double.DenseVector(Nq + 1)
        For i = 0 To Nq
            MesForces(i) = Forces(i)
        Next
        Dim MesDeplacement As New MathNet.Numerics.LinearAlgebra.Double.DenseVector(Nq + 1)
        MesDeplacement = Solve.Solve(MesForces)

        For i = 0 To Nq
            Deplacement(i) = MesDeplacement(i)
        Next

        'la triangularisation du systeme
        '     Dim KglobMatrix(0, 0), ForcesVecteur(0) As double
        '    Call TriangularisationSystem(KglobMatrix, ForcesVecteur)
        'solution
        '   Call TrouverSolution(KglobMatrix, ForcesVecteur)
    End Sub
    Private Sub TrouverContraintes(ChargeTotale As Double)

        For Each ElE In Elements
            ElE.InitiStress(Ngp)
            ElE.InitiStrain(Ngp)
            ElE.LoadDisplacement.Clear()
            ElE.CompressiveStressStrain.Clear()
            ElE.TensileStressStrain.Clear()
        Next

        For i = 0 To Elements.Count - 1
            Call Stress(ChargeTotale, i, 0,,,,, samp)
        Next i

        For j = 0 To Elements.Count - 1
            Dim NodePosition As Integer = ColoserNodeZposition(j)
            Elements.Item(j).ChargerStressStrainCurve()
            Elements.Item(j).ChargerLoadDisplacementCurve(Force.Valeur, Deplacement(NodePosition))
        Next

    End Sub
    Private Sub Stress(ChergeTotale As Double, el As Integer, Optional Comportement As Integer = 0, Optional LoiComportement As Integer = 0, Optional Convergance As Boolean = Nothing,
                       Optional Iter As Integer = 0, Optional MaxIter As Integer = 0, Optional Samp(,) As Double = Nothing, Optional DT As Double = 0)
        Dim ELD(24) As Double

        If IndexNoued.Item(3 * (Elements.Item(el).ListNoeud.Item(0).Ident)) <> -1 Then ELD(1) = Deplacement(IndexNoued.Item(3 * (Elements.Item(el).ListNoeud.Item(0).Ident)))
        If IndexNoued.Item(3 * (Elements.Item(el).ListNoeud.Item(0).Ident) + 1) <> -1 Then ELD(2) = Deplacement(IndexNoued.Item(3 * (Elements.Item(el).ListNoeud.Item(0).Ident) + 1))
        If IndexNoued.Item(3 * (Elements.Item(el).ListNoeud.Item(0).Ident) + 2) <> -1 Then ELD(3) = Deplacement(IndexNoued.Item(3 * (Elements.Item(el).ListNoeud.Item(0).Ident) + 2))

        If IndexNoued.Item(3 * (Elements.Item(el).ListNoeud.Item(1).Ident)) <> -1 Then ELD(4) = Deplacement(IndexNoued.Item(3 * (Elements.Item(el).ListNoeud.Item(1).Ident)))
        If IndexNoued.Item(3 * (Elements.Item(el).ListNoeud.Item(1).Ident) + 1) <> -1 Then ELD(5) = Deplacement(IndexNoued.Item(3 * (Elements.Item(el).ListNoeud.Item(1).Ident) + 1))
        If IndexNoued.Item(3 * (Elements.Item(el).ListNoeud.Item(1).Ident) + 2) <> -1 Then ELD(6) = Deplacement(IndexNoued.Item(3 * (Elements.Item(el).ListNoeud.Item(1).Ident) + 2))

        If IndexNoued.Item(3 * (Elements.Item(el).ListNoeud.Item(2).Ident)) <> -1 Then ELD(7) = Deplacement(IndexNoued.Item(3 * (Elements.Item(el).ListNoeud.Item(2).Ident)))
        If IndexNoued.Item(3 * (Elements.Item(el).ListNoeud.Item(2).Ident) + 1) <> -1 Then ELD(8) = Deplacement(IndexNoued.Item(3 * (Elements.Item(el).ListNoeud.Item(2).Ident) + 1))
        If IndexNoued.Item(3 * (Elements.Item(el).ListNoeud.Item(2).Ident) + 2) <> -1 Then ELD(9) = Deplacement(IndexNoued.Item(3 * (Elements.Item(el).ListNoeud.Item(2).Ident) + 2))

        If IndexNoued.Item(3 * (Elements.Item(el).ListNoeud.Item(3).Ident)) <> -1 Then ELD(10) = Deplacement(IndexNoued.Item(3 * (Elements.Item(el).ListNoeud.Item(3).Ident)))
        If IndexNoued.Item(3 * (Elements.Item(el).ListNoeud.Item(3).Ident) + 1) <> -1 Then ELD(11) = Deplacement(IndexNoued.Item(3 * (Elements.Item(el).ListNoeud.Item(3).Ident) + 1))
        If IndexNoued.Item(3 * (Elements.Item(el).ListNoeud.Item(3).Ident) + 2) <> -1 Then ELD(12) = Deplacement(IndexNoued.Item(3 * (Elements.Item(el).ListNoeud.Item(3).Ident) + 2))

        If IndexNoued.Item(3 * (Elements.Item(el).ListNoeud.Item(4).Ident)) <> -1 Then ELD(13) = Deplacement(IndexNoued.Item(3 * (Elements.Item(el).ListNoeud.Item(4).Ident)))
        If IndexNoued.Item(3 * (Elements.Item(el).ListNoeud.Item(4).Ident) + 1) <> -1 Then ELD(14) = Deplacement(IndexNoued.Item(3 * (Elements.Item(el).ListNoeud.Item(4).Ident) + 1))
        If IndexNoued.Item(3 * (Elements.Item(el).ListNoeud.Item(4).Ident) + 2) <> -1 Then ELD(15) = Deplacement(IndexNoued.Item(3 * (Elements.Item(el).ListNoeud.Item(4).Ident) + 2))

        If IndexNoued.Item(3 * (Elements.Item(el).ListNoeud.Item(5).Ident)) <> -1 Then ELD(16) = Deplacement(IndexNoued.Item(3 * (Elements.Item(el).ListNoeud.Item(5).Ident)))
        If IndexNoued.Item(3 * (Elements.Item(el).ListNoeud.Item(5).Ident) + 1) <> -1 Then ELD(17) = Deplacement(IndexNoued.Item(3 * (Elements.Item(el).ListNoeud.Item(5).Ident) + 1))
        If IndexNoued.Item(3 * (Elements.Item(el).ListNoeud.Item(5).Ident) + 2) <> -1 Then ELD(18) = Deplacement(IndexNoued.Item(3 * (Elements.Item(el).ListNoeud.Item(5).Ident) + 2))

        If IndexNoued.Item(3 * (Elements.Item(el).ListNoeud.Item(6).Ident)) <> -1 Then ELD(19) = Deplacement(IndexNoued.Item(3 * (Elements.Item(el).ListNoeud.Item(6).Ident)))
        If IndexNoued.Item(3 * (Elements.Item(el).ListNoeud.Item(6).Ident) + 1) <> -1 Then ELD(20) = Deplacement(IndexNoued.Item(3 * (Elements.Item(el).ListNoeud.Item(6).Ident) + 1))
        If IndexNoued.Item(3 * (Elements.Item(el).ListNoeud.Item(6).Ident) + 2) <> -1 Then ELD(21) = Deplacement(IndexNoued.Item(3 * (Elements.Item(el).ListNoeud.Item(6).Ident) + 2))

        If IndexNoued.Item(3 * (Elements.Item(el).ListNoeud.Item(7).Ident)) <> -1 Then ELD(22) = Deplacement(IndexNoued.Item(3 * (Elements.Item(el).ListNoeud.Item(7).Ident)))
        If IndexNoued.Item(3 * (Elements.Item(el).ListNoeud.Item(7).Ident) + 1) <> -1 Then ELD(23) = Deplacement(IndexNoued.Item(3 * (Elements.Item(el).ListNoeud.Item(7).Ident) + 1))
        If IndexNoued.Item(3 * (Elements.Item(el).ListNoeud.Item(7).Ident) + 2) <> -1 Then ELD(24) = Deplacement(IndexNoued.Item(3 * (Elements.Item(el).ListNoeud.Item(7).Ident) + 2))
        If Comportement = 0 Then Elements.Item(el).Stress(ChergeTotale, el, Ngp, ELD,,,,,, Samp) 'Lineaire
        If Comportement = 1 Or Comportement = 2 Then
            Elements.Item(el).Stress(ChergeTotale, PropretyBeton.Approch, Ngp, ELD, Comportement, LoiComportement, Convergance, Iter,
                                                          MaxIter, Samp, DT) 'Concrete damaged plasticity
        End If

    End Sub
    Private Sub TriangularisationSystem(ByRef KglobMatrix(,) As Double, ByRef ForcesVecteur() As Double)

        Dim temporary_1 As Double
        Dim multiplier_1 As Double
        Dim line_1 As Long
        Dim Triangular_A(Nq, Nq + 1) As Double

        For i = 0 To Nq
            For j = 0 To Nq
                Triangular_A(i, j) = KGlob(i, j)
            Next
        Next

        For n = 0 To Nq
            Triangular_A(n, Nq + 1) = Forces(n)
        Next n


        For Kg = 0 To Nq - 1

            If Triangular_A(Kg, Kg) = 0 Then
                For n = Kg To Nq
                    If Triangular_A(n, Kg) <> 0 Then line_1 = n : Exit For 'Finds line_1 with non-zero element
                Next n

                For m1 = Kg To Nq + 1
                    temporary_1 = Triangular_A(Kg, m1)
                    Triangular_A(Kg, m1) = Triangular_A(line_1, m1)
                    Triangular_A(line_1, m1) = temporary_1
                Next m1
            End If
            For n = Kg + 1 To Nq
                If Triangular_A(n, Kg) <> 0 Then 'if it is zero, stays as it is
                    multiplier_1 = Triangular_A(n, Kg) / Triangular_A(Kg, Kg)
                    For m1 = Kg To Nq + 1
                        Triangular_A(n, m1) = Triangular_A(n, m1) - Triangular_A(Kg, m1) * multiplier_1
                    Next m1
                End If
            Next n
        Next Kg

        ReDim KglobMatrix(Nq, Nq)
        For n = 0 To Nq
            For m1 = 0 To Nq
                KglobMatrix(m1, n) = Triangular_A(m1, n)
            Next
        Next
        ReDim ForcesVecteur(Nq)
        For n = 0 To Nq
            ForcesVecteur(n) = Triangular_A(n, Nq + 1)
        Next n

    End Sub
    Private Sub TrouverSolution(KglobMatrix(,) As Double, ForcesVecteur() As Double)
        Dim i, j As Integer
        Dim Sum As Double
        Deplacement(Nq) = ForcesVecteur(Nq) / KglobMatrix(Nq, Nq)
        For i = Nq - 1 To 0 Step -1
            Sum = 0
            For j = Nq To i + 1 Step -1
                Sum = Sum + KglobMatrix(i, j) * Deplacement(j)
            Next j
            Deplacement(i) = (ForcesVecteur(i) - Sum) / KglobMatrix(i, j)
        Next i
    End Sub
    Private Function Surface(el As Integer) As Double


        Dim S1 As Double = SurfaceTriangle(Elements.Item(el).ListNoeud(7).Coord(1), Elements.Item(el).ListNoeud(7).Coord(2),
                                               Elements.Item(el).ListNoeud(6).Coord(1), Elements.Item(el).ListNoeud(6).Coord(2),
                                               Elements.Item(el).ListNoeud(5).Coord(1), Elements.Item(el).ListNoeud(5).Coord(2))
        Dim S2 As Double = SurfaceTriangle(Elements.Item(el).ListNoeud(7).Coord(1), Elements.Item(el).ListNoeud(7).Coord(2),
                                               Elements.Item(el).ListNoeud(4).Coord(1), Elements.Item(el).ListNoeud(4).Coord(2),
                                               Elements.Item(el).ListNoeud(5).Coord(1), Elements.Item(el).ListNoeud(5).Coord(2))
        Dim S As Double = S1 + S2
        Return S
    End Function
    Private Function VecteurForceElementaire(el As Integer, F As ConcreteForce, Valeur As Double, FrontElement As Boolean) As Double()
        Dim Fe(24) As Double
        If FrontElement Then
            If F.ChargeRepartie Then
                Dim Rhou As Double = Valeur

                Dim S1 As Double = SurfaceTriangle(Elements.Item(el).ListNoeud(7).Coord(1), Elements.Item(el).ListNoeud(7).Coord(2),
                                               Elements.Item(el).ListNoeud(6).Coord(1), Elements.Item(el).ListNoeud(6).Coord(2),
                                               Elements.Item(el).ListNoeud(5).Coord(1), Elements.Item(el).ListNoeud(5).Coord(2))
                Dim S2 As Double = SurfaceTriangle(Elements.Item(el).ListNoeud(7).Coord(1), Elements.Item(el).ListNoeud(7).Coord(2),
                                               Elements.Item(el).ListNoeud(4).Coord(1), Elements.Item(el).ListNoeud(4).Coord(2),
                                               Elements.Item(el).ListNoeud(5).Coord(1), Elements.Item(el).ListNoeud(5).Coord(2))
                Dim S As Double = S1 + S2
                Dim ForceNodal As Double = Rhou * S / 4

                '   MsgBox("4--> 1:" & Elements.Item(el).ListNoeud(4).Coord(1) & "   2:" & Elements.Item(el).ListNoeud(4).Coord(2) & vbNewLine &
                '         "5--> 1:" & Elements.Item(el).ListNoeud(5).Coord(1) & "   2:" & Elements.Item(el).ListNoeud(5).Coord(2) & vbNewLine &
                '        "6--> 1:" & Elements.Item(el).ListNoeud(6).Coord(1) & "   2:" & Elements.Item(el).ListNoeud(6).Coord(2) & vbNewLine &
                '       "7--> 1:" & Elements.Item(el).ListNoeud(7).Coord(1) & "   2:" & Elements.Item(el).ListNoeud(7).Coord(2))

                Dim ForceNod() As Double = Elements.Item(el).LoadVector(2, Rhou, samp)
                Fe(15) = ForceNod(1) ' ForceNodal
                Fe(18) = ForceNod(2) 'ForceNodal
                Fe(21) = ForceNod(3) ' ForceNodal
                Fe(24) = ForceNod(4) 'ForceNodal

            End If
        Else
            'Charge appliqué au centre de l'eprouvette
            For i = 4 To 7
                If Distance(Elements.Item(el).ListNoeud(i).Coord(1), Elements.Item(el).ListNoeud(i).Coord(2),
                            NoeudProcheCentre.x, NoeudProcheCentre.y) < 0.00001 Then
                    Fe(i * 3 + 2) = Valeur
                End If
            Next
        End If
        Return Fe
    End Function
    Private Sub Gauss(cas As Integer)
        Select Case cas
            Case 1
                samp(1, 1) = 0
                samp(1, 2) = 2
            Case 2
                samp(1, 1) = 1 / Sqrt(3)
                samp(2, 1) = -samp(1, 1)
                samp(1, 2) = 1
                samp(2, 2) = 1

            Case 3
                samp(1, 1) = 0.2 * Sqrt(15)
                samp(2, 1) = 0
                samp(3, 1) = -samp(1, 1)
                samp(1, 2) = 5 / 9
                samp(2, 2) = 8 / 9
                samp(3, 2) = samp(1, 2)
        End Select
    End Sub



    Public Sub ProjectionPointDroite(a As Double, b As Double, c As Double, Xp As Double, Yp As Double, ByRef Xproj As Double, ByRef Yproj As Double)

        If b = 0 Then
            Xproj = -c / a
            Yproj = Yp
            Exit Sub
        End If

        If a = 0 Then
            Yproj = -c / b
            Xproj = Xp
            Exit Sub
        End If
        Dim AA, BB As Double
        AA = -a / b
        BB = -c / b
        Xproj = AA * (Yp + (Xp / AA) - BB) / (AA ^ 2 + 1)
        Yproj = AA * Xproj + BB
100:
    End Sub
    Public Function AppartientSegmentDroite(Xp As Double, Yp As Double, Xd1 As Double, Yd1 As Double, Xd2 As Double, Yd2 As Double,
                                            Optional a As Double = 0, Optional b As Double = 0, Optional c As Double = 0,
                                            Optional ByRef Xproj As Double = 0, Optional ByRef Yproj As Double = 0) As Boolean
        AppartientSegmentDroite = False


        If a = 0 And b = 0 And c = 0 Then
            Call EquationDroiteApartir2Points(Xd1, Yd1, Xd2, Yd2, a, b, c)
        End If



        Call ProjectionPointDroite(a, b, c, Xp, Yp, Xproj, Yproj)
        If Xd1 = 0 And Yd1 = 0 And Xd2 = 0 And Yd2 = 0 Then
            AppartientSegmentDroite = False
            Exit Function
        End If
        Dim Cond1, Cond2 As Boolean
        Cond1 = False
        Cond2 = False

        If Xd1 <> Xd2 Then
            If Xproj <= Max(Xd1, Xd2) And Xproj >= Min(Xd1, Xd2) Then Cond1 = True
        Else
            If Abs(Xproj - Xd1) < 0.001 Then Cond1 = True
        End If
        If Yd1 <> Yd2 Then
            If Yproj <= Max(Yd1, Yd2) And Yproj >= Min(Yd1, Yd2) Then Cond2 = True
        Else
            If Abs(Yproj - Yd1) < 0.001 Then Cond2 = True
        End If
        If Cond1 And Cond2 Then
            AppartientSegmentDroite = True
        End If
    End Function
    Public Function SurfaceTriangle(X1 As Double, Y1 As Double, X2 As Double, Y2 As Double, X3 As Double, Y3 As Double) As Double
        Dim Xproj, Yproj As Double
        Dim DistH As Double
        Dim DistB As Double
        SurfaceTriangle = 0
        If AppartientSegmentDroite(X1, Y1, X2, Y2, X3, Y3,,,, Xproj, Yproj) Then
            DistH = Distance(X1, Y1, Xproj, Yproj)
            DistB = Distance(X2, Y2, X3, Y3)
            SurfaceTriangle = 0.5 * DistH * DistB
            Exit Function
        End If
        If AppartientSegmentDroite(X2, Y2, X1, Y1, X3, Y3,,,, Xproj, Yproj) Then
            DistH = Distance(X2, Y2, Xproj, Yproj)
            DistB = Distance(X1, Y1, X3, Y3)
            SurfaceTriangle = 0.5 * DistH * DistB
            Exit Function
        End If
        If AppartientSegmentDroite(X3, Y3, X2, Y2, X1, Y1,,,, Xproj, Yproj) Then
            DistH = Distance(X3, Y3, Xproj, Yproj)
            DistB = Distance(X2, Y2, X1, Y1)
            SurfaceTriangle = 0.5 * DistH * DistB
            Exit Function
        End If
    End Function

    Private Sub GenerateMeshCylindre(Diametre As Double, Haut As Double, E As Double, V As Double, Fb0_Fc0 As Double, Kc As Double, PsiDegre As Double,
            Fck As Double, Excent As Double, Rhou As Double, Comprission As List(Of DoublePoint), Tension As List(Of DoublePoint))
        Try
            Dim Counteur As List(Of DoublePoint) = CalculerPiremetreCylindre(Diametre)
            Dim OurMesh As Mesh = MeshGenerator(Counteur)
            QuadMesh = QMesh(New QuadMesh(OurMesh))


            'Supprimer double QUAD
            Dim Termine As Boolean = False
            Do Until Termine
                Dim Count As Integer = 0
                Dim ExistQuad As Boolean = False
                For i = 0 To QuadMesh.Count - 1
                    For j = i + 1 To QuadMesh.Count - 1
                        If SameQuad(QuadMesh.Item(i), QuadMesh.Item(j)) Then
                            ExistQuad = True
                            Count = i
                            Exit For
                        End If

                    Next
                    If ExistQuad Then Exit For
                Next
                If ExistQuad Then
                    QuadMesh.Remove(QuadMesh.Item(Count))
                Else
                    Termine = True
                End If
            Loop
            Noeuds.Clear()
            Dim NumbrElemet As Integer = Haut / MaxLong
            Dim Cont As Integer = -1
            Dim LongReal As Double = Haut / NumbrElemet
            NbrNoeudEtage = ListOfVertex.Count
            Dim Dis As Double = 999999999
            Dim Dis1 As Double
            For i = 0 To ListOfVertex.Count - 1
                Dis1 = Sqrt(ListOfVertex.Item(i).x ^ 2 + ListOfVertex.Item(i).y ^ 2)
                If Dis1 < Dis Then
                    NoeudProcheCentre.x = ListOfVertex.Item(i).x
                    NoeudProcheCentre.y = ListOfVertex.Item(i).y
                End If
            Next


            Dim Z As Double
            Do Until Z > Haut
                For i = 0 To ListOfVertex.Count - 1
                    Cont = Cont + 1
                    Dim NewNoeud As New Node
                    NewNoeud.Ident = Cont
                    NewNoeud.Coord(1) = ListOfVertex.Item(i).x
                    NewNoeud.Coord(2) = ListOfVertex.Item(i).y
                    NewNoeud.Coord(3) = Z
                    Noeuds.Add(NewNoeud)
                Next

                Z = Z + LongReal
                If Abs(Z - Haut) < 0.0001 Then Z = Haut
            Loop
            Z = LongReal
            Elements.Clear()
            Cont = -1
            Dim Etage, NumberNoued As Integer
            Do Until Z > Haut
                For i = 0 To QuadMesh.Count - 1
                    Cont = Cont + 1
                    Dim ListNoeud As New List(Of Node)
                    NumberNoued = QuadMesh.Item(i).S1 + Etage * ListOfVertex.Count
                    ListNoeud.Add(Noeuds.Item(NumberNoued))

                    NumberNoued = QuadMesh.Item(i).S2 + Etage * ListOfVertex.Count
                    ListNoeud.Add(Noeuds.Item(NumberNoued))

                    NumberNoued = QuadMesh.Item(i).S3 + Etage * ListOfVertex.Count
                    ListNoeud.Add(Noeuds.Item(NumberNoued))

                    NumberNoued = QuadMesh.Item(i).S4 + Etage * ListOfVertex.Count
                    ListNoeud.Add(Noeuds.Item(NumberNoued))

                    NumberNoued = QuadMesh.Item(i).S1 + (Etage + 1) * ListOfVertex.Count
                    ListNoeud.Add(Noeuds.Item(NumberNoued))

                    NumberNoued = QuadMesh.Item(i).S2 + (Etage + 1) * ListOfVertex.Count
                    ListNoeud.Add(Noeuds.Item(NumberNoued))

                    NumberNoued = QuadMesh.Item(i).S3 + (Etage + 1) * ListOfVertex.Count
                    ListNoeud.Add(Noeuds.Item(NumberNoued))

                    NumberNoued = QuadMesh.Item(i).S4 + (Etage + 1) * ListOfVertex.Count
                    ListNoeud.Add(Noeuds.Item(NumberNoued))

                    Elements.Add(New BrickEightNodes(Cont, E, V, Fb0_Fc0, Kc, PsiDegre, Fck, Excent, Rhou, Comprission, Tension, ListNoeud))
                Next
                Z = Z + LongReal
                If Abs(Z - Haut) < 0.0001 Then Z = Haut
                Etage = Etage + 1
            Loop
        Catch
            MsgBox("Mesh Error")
        End Try
    End Sub
    Private Function CalculerPiremetreCylindre(Diametre As Double) As List(Of DoublePoint)
        Dim R As Double = Diametre / 2
        Dim angle As Double
        Dim radian As Double
        Dim Resulat As New List(Of DoublePoint)
        Do Until angle >= 360
            Dim NewPoint As DoublePoint
            radian = angle * PI / 180
            NewPoint.x = R * Cos(radian) + R
            NewPoint.y = R * Sin(radian) + R
            Resulat.Add(NewPoint)
            angle = angle + MaillgeParametre
        Loop
        MaxLong = (R) * Sin(MaillgeParametre * PI / 180)
        Lmin = MaxLong
        Return Resulat
    End Function
    Private Sub PointFromDistance(P1 As DoublePoint, P2 As DoublePoint, Dist As Double, ByRef P As DoublePoint)
        If Abs(P1.x - P2.x) < 0.0001 Then
            P.x = P1.x
            P.y = P1.y + Dist
            Exit Sub
        End If
        If Abs(P1.y - P2.y) < 0.0001 Then
            P.x = P1.x + Dist
            P.y = P1.y
            Exit Sub
        End If
    End Sub
    Private Function CalculatePointsCube(YLar As Double, XLon As Double, ByRef LineNumber As Integer) As List(Of DoublePoint)
        Dim DisY As Double = YLar * MaillgeParametre / 100
        Lmin = DisY
        DisY = YLar / DisY
        DisY = YLar / (Int(DisY))
        Dim DisX As Double = XLon / DisY
        DisX = XLon / (Int(DisX))
        '     DisY = 0.25
        '   DisX = 0.25
        MaxLong = Max(DisY, DisX)

        Dim Resulat As New List(Of DoublePoint)
        Dim P As New List(Of DoublePoint)
        Dim DisCour, Dtot As Double
        Dim Cont As Integer = 0
        Dtot = XLon

        Dim P1 As New DoublePoint With {.x = 0, .y = 0}
        Dim P2 As New DoublePoint With {.x = XLon, .y = 0}



        Do
            Dim NewP As DoublePoint
            PointFromDistance(P1, P2, DisCour, NewP)
            P.Add(NewP)
            DisCour = DisCour + DisX
            If DisCour >= Dtot Then
                Dim NewP2 As DoublePoint
                PointFromDistance(P1, P2, Dtot, NewP2)
                P.Add(NewP2)
                Exit Do
            End If
        Loop


        LineNumber = P.Count
        Dtot = YLar
        DisCour = 0
        Do
            For i = 0 To P.Count - 1
                Dim NewP As DoublePoint
                NewP.x = P.Item(i).x
                NewP.y = DisCour
                Resulat.Add(NewP)
            Next

            DisCour = DisCour + DisY
            If DisCour >= Dtot Then
                For i = 0 To P.Count - 1
                    Dim NewP As DoublePoint
                    NewP.x = P.Item(i).x
                    NewP.y = Dtot
                    Resulat.Add(NewP)
                Next
                Exit Do
            End If
        Loop

        Return Resulat
    End Function
    Private Function QMesh(Segmts As QuadMesh) As List(Of Quadralateral)
        Dim ListSegments As New List(Of Segment)
        Dim ListQmeshCalculated As List(Of Quadralateral)
        Dim Resulat As New List(Of Quadralateral)
        ListOfVertex.Clear()
        For Each Seg In Segmts.Edges
            Dim AddSeg As Segment
            AddSeg.P1.x = Segmts.Vertices.Item(Seg.P0).X
            AddSeg.P1.y = Segmts.Vertices.Item(Seg.P0).Y
            AddSeg.P2.x = Segmts.Vertices.Item(Seg.P1).X
            AddSeg.P2.y = Segmts.Vertices.Item(Seg.P1).Y
            If Not ExistSegment(AddSeg, ListSegments) Then ListSegments.Add(AddSeg)
        Next
        For Each v In Segmts.Vertices
            AddVertex(v.X, v.Y)
        Next

        Do Until ListSegments.Count = 0
            ListQmeshCalculated = QmeshFromSeg(ListSegments.Item(0), ListSegments)
            ListSegments.Remove(ListSegments.Item(0))
            For Each El In ListQmeshCalculated
                Resulat.Add(El)
            Next
        Loop
        Return Resulat
    End Function

    Private Sub AddVertex(x As Double, y As Double)
        For Each v In ListOfVertex
            If Abs(x - v.x) < 0.0001 And Abs(y - v.y) < 0.0001 Then
                Exit Sub
            End If
        Next
        Dim Addv As DoublePoint
        Addv.x = x
        Addv.y = y
        ListOfVertex.Add(Addv)
    End Sub
    Private Function ExistSegment(P1 As DoublePoint, P2 As DoublePoint, ListSeg As List(Of Segment)) As Boolean
        ExistSegment = False
        For Each Seg In ListSeg
            If SameSegment(P1, P2, Seg) Then
                ExistSegment = True
                Exit Function
            End If
        Next
    End Function
    Private Function ExistSegment(S As Segment, ListSeg As List(Of Segment)) As Boolean
        ExistSegment = False
        For Each Seg In ListSeg
            If SameSegment(S, Seg) Then
                ExistSegment = True
                Exit Function
            End If
        Next
    End Function
    Private Function QmeshFromSeg(S As Segment, TotalSegment As List(Of Segment)) As List(Of Quadralateral)
        Dim ConectedSegment As List(Of Segment) = ListOfSegmentConected(S, TotalSegment)
        Dim Result As New List(Of Quadralateral)
        For Each S1 In ConectedSegment
            For Each S2 In ConectedSegment
                If Not SameSegment(S1, S2) Then
                    Dim P1 As DoublePoint = ChargerDeuxiemPoint(S, S1)
                    Dim P2 As DoublePoint = ChargerDeuxiemPoint(S, S2)
                    If ExistSegment(P1, P2, TotalSegment) Then
                        Dim Qelement As Quadralateral

                        Qelement.S1 = IndexDoublePoint(S.P1)
                        Qelement.S2 = IndexDoublePoint(S.P2)
                        If SameDoublePoint(S1.P1, S.P2) Or SameDoublePoint(S1.P2, S.P2) Then
                            Qelement.S3 = IndexDoublePoint(P1)
                            Qelement.S4 = IndexDoublePoint(P2)
                        Else
                            Qelement.S3 = IndexDoublePoint(P2)
                            Qelement.S4 = IndexDoublePoint(P1)
                        End If
                        Result.Add(Qelement)
                    End If
                End If
            Next
        Next
        Return Result
    End Function
    Public Function IndexDoublePoint(P As DoublePoint) As Integer
        IndexDoublePoint = 0
        For Each v In ListOfVertex
            If Abs(P.x - v.x) < 0.0001 And Abs(P.y - v.y) < 0.0001 Then
                Return ListOfVertex.IndexOf(v)
            End If
        Next
    End Function
    Private Function ChargerDeuxiemPoint(S As Segment, Sconct As Segment) As DoublePoint
        If Not SameDoublePoint(Sconct.P1, S.P1) And Not SameDoublePoint(Sconct.P1, S.P2) Then
            Return Sconct.P1
        End If
        If Not SameDoublePoint(Sconct.P2, S.P1) And Not SameDoublePoint(Sconct.P2, S.P2) Then
            Return Sconct.P2
        End If
    End Function
    Private Function ListOfSegmentConected(MySegment As Segment, TotalSegment As List(Of Segment)) As List(Of Segment)
        Dim Resulat As New List(Of Segment)
        For Each S In TotalSegment
            If Not SameSegment(S, MySegment) Then
                If ConnectedSegment(S, MySegment) Then
                    Resulat.Add(S)
                End If
            End If
        Next
        Return Resulat
    End Function
    Public Function ConnectedSegment(S1 As Segment, S2 As Segment) As Boolean
        ConnectedSegment = False
        If Abs(S1.P1.x - S2.P1.x) < 0.0001 And Abs(S1.P1.y - S2.P1.y) < 0.0001 Then
            ConnectedSegment = True
        End If
        If Abs(S1.P2.x - S2.P2.x) < 0.0001 And Abs(S1.P2.y - S2.P2.y) < 0.0001 Then
            ConnectedSegment = True
        End If
        If Abs(S1.P2.x - S2.P1.x) < 0.0001 And Abs(S1.P2.y - S2.P1.y) < 0.0001 Then
            ConnectedSegment = True
        End If
        If Abs(S1.P1.x - S2.P2.x) < 0.0001 And Abs(S1.P1.y - S2.P2.y) < 0.0001 Then
            ConnectedSegment = True
        End If
    End Function
    Public Function SameDoublePoint(P1 As DoublePoint, P2 As DoublePoint) As Boolean
        SameDoublePoint = False
        If Abs(P1.x - P2.x) < 0.0001 And Abs(P1.y - P2.y) < 0.0001 Then
            SameDoublePoint = True
        End If
    End Function
    Public Function SameSegment(P1 As DoublePoint, P2 As DoublePoint, S As Segment) As Boolean
        SameSegment = False
        If Abs(S.P1.x - P1.x) < 0.0001 And Abs(S.P1.y - P1.y) < 0.0001 Then
            If Abs(S.P2.x - P2.x) < 0.0001 And Abs(S.P2.y - P2.y) < 0.0001 Then
                SameSegment = True
            End If
        End If
        If Abs(S.P2.x - P1.x) < 0.0001 And Abs(S.P2.y - P1.y) < 0.0001 Then
            If Abs(S.P1.x - P2.x) < 0.0001 And Abs(S.P1.y - P2.y) < 0.0001 Then
                SameSegment = True
            End If
        End If
    End Function
    Public Function SameSegment(S1 As Segment, S2 As Segment) As Boolean
        SameSegment = False
        If Abs(S1.P1.x - S2.P1.x) < 0.0001 And Abs(S1.P1.y - S2.P1.y) < 0.0001 Then
            If Abs(S1.P2.x - S2.P2.x) < 0.0001 And Abs(S1.P2.y - S2.P2.y) < 0.0001 Then
                SameSegment = True
            End If
        End If
        If Abs(S1.P2.x - S2.P1.x) < 0.0001 And Abs(S1.P2.y - S2.P1.y) < 0.0001 Then
            If Abs(S1.P1.x - S2.P2.x) < 0.0001 And Abs(S1.P1.y - S2.P2.y) < 0.0001 Then
                SameSegment = True
            End If
        End If
    End Function
    Private Function MeshGenerator(Polygon As List(Of DoublePoint)) As Mesh
        Dim Resulat As Mesh
        Dim MyPolygon As New Polygon
        Dim VertexCount As New List(Of Vertex)
        For i = 0 To Polygon.Count - 1
            MyPolygon.Add(New Vertex(Polygon.Item(i).x, Polygon.Item(i).y))
        Next


        '   MyPolygon.Add(New Contour(VertexCount, 1))
        Dim Options As New ConstraintOptions
        Dim Quality As New QualityOptions()
        Quality.MaximumAngle = 130
        Quality.MinimumAngle = 25

        Quality.MaximumArea = MaxLong * MaxLong

        Resulat = MyPolygon.Triangulate(Options, Quality)

        Return Resulat
    End Function
    Private Function SameQuad(Q1 As Quadralateral, Q2 As Quadralateral) As Boolean
        If ExistNoad(Q1.S1, Q2) And ExistNoad(Q1.S2, Q2) And ExistNoad(Q1.S3, Q2) And ExistNoad(Q1.S4, Q2) Then
            Return True
        Else
            Return False
        End If
    End Function
    Private Function ExistNoad(N As Integer, Q As Quadralateral) As Boolean
        If N = Q.S1 Or N = Q.S2 Or N = Q.S3 Or N = Q.S4 Then
            Return True
        Else
            Return False
        End If
    End Function
    Public Sub Draw2D(PrintColor As Color)
        If IsNothing(QuadMesh) Then Exit Sub
        Dim ContIndex As Integer = 0

        GL.PushMatrix()
        GL.Color3(PrintColor)
        GL.Disable(EnableCap.Light0)
        GL.Disable(EnableCap.LineSmooth)
        GL.LineWidth(1)
        For Each Quad In QuadMesh
            GL.Begin(PrimitiveType.LineLoop)
            GL.Vertex2(ListOfVertex.Item(Quad.S1).x, ListOfVertex.Item(Quad.S1).y)
            GL.Vertex2(ListOfVertex.Item(Quad.S2).x, ListOfVertex.Item(Quad.S2).y)
            GL.Vertex2(ListOfVertex.Item(Quad.S3).x, ListOfVertex.Item(Quad.S3).y)
            GL.Vertex2(ListOfVertex.Item(Quad.S4).x, ListOfVertex.Item(Quad.S4).y)
            GL.End()
        Next
        GL.PopMatrix()
    End Sub
    Private Sub DessignerCube(Noued1 As Node, Noued2 As Node, Noued3 As Node, Noued4 As Node,
                              Noued5 As Node, Noued6 As Node, Noued7 As Node, Noued8 As Node)
        GL.Begin(PrimitiveType.Polygon)
        GL.Vertex3(Noued1.Coord(1), Noued1.Coord(2), Noued1.Coord(3))
        GL.Vertex3(Noued2.Coord(1), Noued2.Coord(2), Noued2.Coord(3))
        GL.Vertex3(Noued3.Coord(1), Noued3.Coord(2), Noued3.Coord(3))
        GL.Vertex3(Noued4.Coord(1), Noued4.Coord(2), Noued4.Coord(3))
        GL.End()

        GL.Begin(PrimitiveType.Polygon)
        GL.Vertex3(Noued5.Coord(1), Noued5.Coord(2), Noued5.Coord(3))
        GL.Vertex3(Noued6.Coord(1), Noued6.Coord(2), Noued6.Coord(3))
        GL.Vertex3(Noued7.Coord(1), Noued7.Coord(2), Noued7.Coord(3))
        GL.Vertex3(Noued8.Coord(1), Noued8.Coord(2), Noued8.Coord(3))
        GL.End()

        GL.Begin(PrimitiveType.Polygon)
        GL.Vertex3(Noued1.Coord(1), Noued1.Coord(2), Noued1.Coord(3))
        GL.Vertex3(Noued2.Coord(1), Noued2.Coord(2), Noued2.Coord(3))
        GL.Vertex3(Noued6.Coord(1), Noued6.Coord(2), Noued6.Coord(3))
        GL.Vertex3(Noued5.Coord(1), Noued5.Coord(2), Noued5.Coord(3))
        GL.End()

        GL.Begin(PrimitiveType.Polygon)
        GL.Vertex3(Noued3.Coord(1), Noued3.Coord(2), Noued3.Coord(3))
        GL.Vertex3(Noued4.Coord(1), Noued4.Coord(2), Noued4.Coord(3))
        GL.Vertex3(Noued8.Coord(1), Noued8.Coord(2), Noued8.Coord(3))
        GL.Vertex3(Noued7.Coord(1), Noued7.Coord(2), Noued7.Coord(3))
        GL.End()
    End Sub

    Private Sub DessignerLimiteCube(Noued1 As Node, Noued2 As Node, Noued3 As Node, Noued4 As Node,
                              Noued5 As Node, Noued6 As Node, Noued7 As Node, Noued8 As Node)
        GL.Begin(PrimitiveType.LineLoop)
        GL.Vertex3(Noued1.Coord(1), Noued1.Coord(2), Noued1.Coord(3))
        GL.Vertex3(Noued2.Coord(1), Noued2.Coord(2), Noued2.Coord(3))
        GL.Vertex3(Noued6.Coord(1), Noued6.Coord(2), Noued6.Coord(3))
        GL.Vertex3(Noued5.Coord(1), Noued5.Coord(2), Noued5.Coord(3))
        GL.End()

        GL.Begin(PrimitiveType.LineLoop)
        GL.Vertex3(Noued3.Coord(1), Noued3.Coord(2), Noued3.Coord(3))
        GL.Vertex3(Noued7.Coord(1), Noued7.Coord(2), Noued7.Coord(3))
        GL.Vertex3(Noued6.Coord(1), Noued6.Coord(2), Noued6.Coord(3))
        GL.Vertex3(Noued2.Coord(1), Noued2.Coord(2), Noued2.Coord(3))
        GL.End()

        GL.Begin(PrimitiveType.LineLoop)
        GL.Vertex3(Noued1.Coord(1), Noued1.Coord(2), Noued1.Coord(3))
        GL.Vertex3(Noued2.Coord(1), Noued2.Coord(2), Noued2.Coord(3))
        GL.Vertex3(Noued6.Coord(1), Noued6.Coord(2), Noued6.Coord(3))
        GL.Vertex3(Noued5.Coord(1), Noued5.Coord(2), Noued5.Coord(3))
        GL.End()


    End Sub

    Private Sub DessignerCubeDeplacer(Noued1 As Node, Noued2 As Node, Noued3 As Node, Noued4 As Node, Noued5 As Node,
                                      Noued6 As Node, Noued7 As Node, Noued8 As Node, Echelle As Integer)
        Dim DepX, DepY, DepZ As Double



        GL.Begin(PrimitiveType.Polygon)
        DeplacementNoeud(Noued1.Ident, Echelle, DepX, DepY, DepZ)
        GL.Vertex3(Noued1.Coord(1) + DepX, Noued1.Coord(2) + DepY, Noued1.Coord(3) + DepZ)
        DeplacementNoeud(Noued2.Ident, Echelle, DepX, DepY, DepZ)
        GL.Vertex3(Noued2.Coord(1) + DepX, Noued2.Coord(2) + DepY, Noued2.Coord(3) + DepZ)
        DeplacementNoeud(Noued3.Ident, Echelle, DepX, DepY, DepZ)
        GL.Vertex3(Noued3.Coord(1) + DepX, Noued3.Coord(2) + DepY, Noued3.Coord(3) + DepZ)
        DeplacementNoeud(Noued4.Ident, Echelle, DepX, DepY, DepZ)
        GL.Vertex3(Noued4.Coord(1) + DepX, Noued4.Coord(2) + DepY, Noued4.Coord(3) + DepZ)
        GL.End()

        GL.Begin(PrimitiveType.Polygon)
        DeplacementNoeud(Noued5.Ident, Echelle, DepX, DepY, DepZ)
        GL.Vertex3(Noued5.Coord(1) + DepX, Noued5.Coord(2) + DepY, Noued5.Coord(3) + DepZ)
        DeplacementNoeud(Noued6.Ident, Echelle, DepX, DepY, DepZ)
        GL.Vertex3(Noued6.Coord(1) + DepX, Noued6.Coord(2) + DepY, Noued6.Coord(3) + DepZ)
        DeplacementNoeud(Noued7.Ident, Echelle, DepX, DepY, DepZ)
        GL.Vertex3(Noued7.Coord(1) + DepX, Noued7.Coord(2) + DepY, Noued7.Coord(3) + DepZ)
        DeplacementNoeud(Noued8.Ident, Echelle, DepX, DepY, DepZ)
        GL.Vertex3(Noued8.Coord(1) + DepX, Noued8.Coord(2) + DepY, Noued8.Coord(3) + DepZ)
        GL.End()

        GL.Begin(PrimitiveType.Polygon)
        DeplacementNoeud(Noued1.Ident, Echelle, DepX, DepY, DepZ)
        GL.Vertex3(Noued1.Coord(1) + DepX, Noued1.Coord(2) + DepY, Noued1.Coord(3) + DepZ)
        DeplacementNoeud(Noued2.Ident, Echelle, DepX, DepY, DepZ)
        GL.Vertex3(Noued2.Coord(1) + DepX, Noued2.Coord(2) + DepY, Noued2.Coord(3) + DepZ)
        DeplacementNoeud(Noued6.Ident, Echelle, DepX, DepY, DepZ)
        GL.Vertex3(Noued6.Coord(1) + DepX, Noued6.Coord(2) + DepY, Noued6.Coord(3) + DepZ)
        DeplacementNoeud(Noued5.Ident, Echelle, DepX, DepY, DepZ)
        GL.Vertex3(Noued5.Coord(1) + DepX, Noued5.Coord(2) + DepY, Noued5.Coord(3) + DepZ)
        GL.End()

        GL.Begin(PrimitiveType.Polygon)
        DeplacementNoeud(Noued3.Ident, Echelle, DepX, DepY, DepZ)
        GL.Vertex3(Noued3.Coord(1) + DepX, Noued3.Coord(2) + DepY, Noued3.Coord(3) + DepZ)
        DeplacementNoeud(Noued4.Ident, Echelle, DepX, DepY, DepZ)
        GL.Vertex3(Noued4.Coord(1) + DepX, Noued4.Coord(2) + DepY, Noued4.Coord(3) + DepZ)
        DeplacementNoeud(Noued8.Ident, Echelle, DepX, DepY, DepZ)
        GL.Vertex3(Noued8.Coord(1) + DepX, Noued8.Coord(2) + DepY, Noued8.Coord(3) + DepZ)
        DeplacementNoeud(Noued7.Ident, Echelle, DepX, DepY, DepZ)
        GL.Vertex3(Noued7.Coord(1) + DepX, Noued7.Coord(2) + DepY, Noued7.Coord(3) + DepZ)
        GL.End()
    End Sub
    Public Sub Draw3D(InitialShape As Boolean, PrintColor As Color, Optional Echelle As Integer = 10)
        If IsNothing(QuadMesh) Then Exit Sub
        Dim ContIndex As Integer = 0
        GL.Enable(EnableCap.Light0)
        GL.Enable(EnableCap.LineSmooth)
        If InitialShape Then
        
            GL.PushMatrix()

            For Each Elem In Elements

                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line)

                GL.Color3(Color.BlueViolet)
                DessignerCube(Elem.ListNoeud.Item(0), Elem.ListNoeud.Item(1), Elem.ListNoeud.Item(2), Elem.ListNoeud.Item(3),
              Elem.ListNoeud.Item(4), Elem.ListNoeud.Item(5), Elem.ListNoeud.Item(6), Elem.ListNoeud.Item(7))

                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill)
                GL.Color3(Color.Brown)
                DessignerCube(Elem.ListNoeud.Item(0), Elem.ListNoeud.Item(1), Elem.ListNoeud.Item(2), Elem.ListNoeud.Item(3),
                              Elem.ListNoeud.Item(4), Elem.ListNoeud.Item(5), Elem.ListNoeud.Item(6), Elem.ListNoeud.Item(7))


            Next
            GL.PopMatrix()

        Else

            GL.PushMatrix()

            For Each Elem In Elements

                GL.LineWidth(1)
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line)

                GL.Color3(Color.DarkBlue)
                DessignerCubeDeplacer(Elem.ListNoeud.Item(0), Elem.ListNoeud.Item(1), Elem.ListNoeud.Item(2), Elem.ListNoeud.Item(3),
                              Elem.ListNoeud.Item(4), Elem.ListNoeud.Item(5), Elem.ListNoeud.Item(6), Elem.ListNoeud.Item(7), Echelle)
                GL.LineWidth(2)
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill)
                GL.Color3(PrintColor)
                DessignerCubeDeplacer(Elem.ListNoeud.Item(0), Elem.ListNoeud.Item(1), Elem.ListNoeud.Item(2), Elem.ListNoeud.Item(3),
                             Elem.ListNoeud.Item(4), Elem.ListNoeud.Item(5), Elem.ListNoeud.Item(6), Elem.ListNoeud.Item(7), Echelle)

                GL.LineWidth(1)
            Next
            GL.PopMatrix()
        End If
    End Sub
    Private Sub DeplacementNoeud(Ident As Integer, Echelle As Integer, ByRef DepX As Double, ByRef DepY As Double, ByRef DepZ As Double)
        Dim I, II As Integer
        If IsNothing(OurProblem.Deplacement) Then Exit Sub
        Try
            I = 3 * Ident
            DepX = 0
            DepY = 0
            DepZ = 0
            If IndexNoued.Count = 0 Then Exit Sub
            II = OurProblem.IndexNoued.Item(I)
            If II <> -1 Then DepX = OurProblem.Deplacement(II) * Echelle
            II = OurProblem.IndexNoued.Item(I + 1)
            If II <> -1 Then DepY = OurProblem.Deplacement(II) * Echelle
            II = OurProblem.IndexNoued.Item(I + 2)
            If II <> -1 Then DepZ = OurProblem.Deplacement(II) * Echelle
        Catch
            DepX = 0
            DepY = 0
            DepZ = 0
        End Try
    End Sub
    Private Sub GenerateMeshCube(ZHaut As Double, Ylar As Double, XLon As Double, E As Double, V As Double, Fb0_Fc0 As Double, Kc As Double, PsiDegre As Double,
            Fck As Double, Excent As Double, Rhou As Double, Comprission As List(Of DoublePoint), Tension As List(Of DoublePoint))
        Dim LineNumber As Integer = 0


        ListOfVertex.Clear()
        ListOfVertex = CalculatePointsCube(Ylar, XLon, LineNumber)

        Dim Dtot As Double = Ylar
        Dim DisY As Double = Ylar * MaillgeParametre / 100
        DisY = Ylar / DisY
        DisY = Ylar / (Int(DisY))

        Dim DisCour As Double = DisY
        Dim Cont As Integer = 1
        QuadMesh.Clear()

        Do
            For i = 0 To LineNumber - 2
                QuadMesh.Add(New Quadralateral With {.S1 = i + (Cont - 1) * LineNumber, .S2 = i + (Cont - 1) * LineNumber + 1,
                             .S3 = i + Cont * LineNumber + 1, .S4 = i + Cont * LineNumber})
            Next
            Cont = Cont + 1
            If DisCour >= Dtot Then Exit Do


            DisCour = DisCour + DisY
            If DisCour >= Dtot Then
                For i = 0 To LineNumber - 2
                    QuadMesh.Add(New Quadralateral With {.S1 = i + (Cont - 1) * LineNumber, .S2 = i + (Cont - 1) * LineNumber + 1,
                             .S3 = i + Cont * LineNumber + 1, .S4 = i + Cont * LineNumber})
                Next
                Exit Do
            End If
        Loop

        'Delete double QUAD
        Dim Termine As Boolean = False
        Do Until Termine
            Dim Count As Integer = 0
            Dim ExistQuad As Boolean = False
            For i = 0 To QuadMesh.Count - 1
                For j = i + 1 To QuadMesh.Count - 1
                    If SameQuad(QuadMesh.Item(i), QuadMesh.Item(j)) Then
                        ExistQuad = True
                        Count = i
                        Exit For
                    End If

                Next
                If ExistQuad Then Exit For
            Next
            If ExistQuad Then
                QuadMesh.Remove(QuadMesh.Item(Count))
            Else
                Termine = True
            End If
        Loop
        Noeuds.Clear()
        Dim NumbrElemet As Integer = Haut / MaxLong

        Dim LongReal As Double = Haut / NumbrElemet

        NbrNoeudEtage = ListOfVertex.Count
        Dim Dis As Double = 999999999
        Dim Dis1 As Double
        For i = 0 To ListOfVertex.Count - 1
            Dis1 = Sqrt(ListOfVertex.Item(i).x ^ 2 + ListOfVertex.Item(i).y ^ 2)
            If Dis1 < Dis Then
                NoeudProcheCentre.x = ListOfVertex.Item(i).x
                NoeudProcheCentre.y = ListOfVertex.Item(i).y
            End If
        Next

        Cont = -1
        Dim Z As Double
        Do Until Z > Haut
            For i = 0 To ListOfVertex.Count - 1
                Cont = Cont + 1
                Dim NewNoeud As New Node
                NewNoeud.Ident = Cont
                NewNoeud.Coord(1) = ListOfVertex.Item(i).x
                NewNoeud.Coord(2) = ListOfVertex.Item(i).y
                NewNoeud.Coord(3) = Z
                Noeuds.Add(NewNoeud)
            Next

            Z = Z + LongReal
            If Abs(Z - Haut) < 0.0001 Then Z = Haut
        Loop
        Z = LongReal
        Cont = -1
        Elements.Clear()
        Dim Etage, NumberNoued As Integer
        Do Until Z > Haut
            For i = 0 To QuadMesh.Count - 1
                Cont = Cont + 1

                Dim ListNoeud As New List(Of Node)

                NumberNoued = QuadMesh.Item(i).S1 + Etage * ListOfVertex.Count  ' Noeud 01
                ListNoeud.Add(Noeuds.Item(NumberNoued))

                NumberNoued = QuadMesh.Item(i).S2 + Etage * ListOfVertex.Count  ' Noeud 02
                ListNoeud.Add(Noeuds.Item(NumberNoued))

                NumberNoued = QuadMesh.Item(i).S3 + Etage * ListOfVertex.Count  'Noeud 03
                ListNoeud.Add(Noeuds.Item(NumberNoued))

                NumberNoued = QuadMesh.Item(i).S4 + Etage * ListOfVertex.Count  ' Noeud 04
                ListNoeud.Add(Noeuds.Item(NumberNoued))

                NumberNoued = QuadMesh.Item(i).S1 + (Etage + 1) * ListOfVertex.Count ' Noeud 05
                ListNoeud.Add(Noeuds.Item(NumberNoued))

                NumberNoued = QuadMesh.Item(i).S2 + (Etage + 1) * ListOfVertex.Count   'Noeud 06
                ListNoeud.Add(Noeuds.Item(NumberNoued))

                NumberNoued = QuadMesh.Item(i).S3 + (Etage + 1) * ListOfVertex.Count  'Noeud 07
                ListNoeud.Add(Noeuds.Item(NumberNoued))

                NumberNoued = QuadMesh.Item(i).S4 + (Etage + 1) * ListOfVertex.Count ' Noeud 08
                ListNoeud.Add(Noeuds.Item(NumberNoued))

                Elements.Add(New BrickEightNodes(Cont, E, V, Fb0_Fc0, Kc, PsiDegre, Fck, Excent, Rhou, Comprission, Tension, ListNoeud))
            Next
            Z = Z + LongReal
            If Abs(Z - Haut) < 0.0001 Then Z = Haut
            Etage = Etage + 1
        Loop
    End Sub
End Class

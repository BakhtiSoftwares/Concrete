Imports System.Math
Imports MathNet.Numerics.LinearAlgebra

Public Class BrickEightNodes
    Inherits Base
    Public Ident As Integer
    Public ListNoeud As List(Of Node)
    Public Material As New PDM
    Public DEE(6, 6), FUN(8), FUNQuad(4), DER(3, 8), DerQuad(2, 4), DERIV(3, 8), DerQuadIV(2, 4), JAC(3, 3), JAC1(3, 3), JACQuad(2, 2), JACQuad1(2, 2), Ke(24, 24), Fe(24) As Double
    Public BEE(6, 24), BT(24, 6), Eload(24), Bload(24), DBEE(6, 24), BTDB(24, 24) As Double
    Public SigmaX(), DamageParameter(), SigmaY(), SigmaZ(), TouXY(), TouYZ(), TouZX() As Double


    Public DefoX(), DefoY(), DefoZ(), DefoXY(), DefoYZ(), DefoZX() As Double

    Public Comprission As List(Of DoublePoint)
    Public Tension As List(Of DoublePoint)
    Public LoadDisplacement As List(Of DoublePoint)
    Public ListOfDamageParameter As List(Of Double)
    Public CompressiveStressStrain As List(Of DoublePoint)
    Public TensileStressStrain As List(Of DoublePoint)
    Sub New(_Ident As Integer, _E As Double, _V As Double, _Fb0_Fc0 As Double, _Kc As Double, _PsiDegre As Double,
            _Fck As Double, _Excent As Double, _Rhou As Double, _Comprission As List(Of DoublePoint),
           _Tension As List(Of DoublePoint), _ListNoeud As List(Of Node))
        Ident = _Ident
        ListNoeud = _ListNoeud
        Material.V = _V
        Material.Fb0_Fc0 = _Fb0_Fc0
        Material.Kc = _Kc
        Material.PsiDegre = _PsiDegre
        Material.Fck = _Fck
        Material.Excent = _Excent
        Tension = _Tension
        Comprission = _Comprission

        If _E <> 0 Then
            Material.E0 = _E
        Else
            Material.E0 = ModuleElasticite(Material.Fck)
        End If
        Material.Leq = CalculLeq()
        ComputeBcAndBt(0.0001)
        If IsNothing(LoadDisplacement) Then LoadDisplacement = New List(Of DoublePoint)
        If IsNothing(ListOfDamageParameter) Then ListOfDamageParameter = New List(Of Double)

        If IsNothing(CompressiveStressStrain) Then CompressiveStressStrain = New List(Of DoublePoint)
        If IsNothing(TensileStressStrain) Then TensileStressStrain = New List(Of DoublePoint)
    End Sub

    Sub ComputeBcAndBt(Tol As Double)
        Dim Fcm, Fc0, Ftm, Defc1, Eci, E0, Ft0, Gf, Gch As Double
        Dim SigmaC, OldDefSigmaC, bcStep As Double
        Fcm = Material.Fck + 8
        Ftm = 0.3016 * Material.Fck ^ (2 / 3)
        Eci = 10000 * Fcm ^ (1 / 3)
        E0 = Eci * (0.8 + 0.2 * (Fcm / 88))
        Gf = 0.073 * Fcm ^ 0.18
        Gch = Gf * (Fcm / Ftm) ^ 2
        Fc0 = 0.4 * Fcm
        Material.ac = 7.873
        Material.at = 1
        Defc1 = 0.0007 * Fcm ^ 0.31
        If Defc1 > 0.0028 Then Defc1 = 0.0028
        Defc1 -= Fcm / E0
        Ft0 = 0.3016 * Material.Fck ^ (2 / 3)
        bcStep = 10
        Do
            Material.bc += bcStep
            SigmaC = Fc0 * ((1 + Material.ac) * Exp(-Material.bc * Defc1) - Material.ac * Exp(-2 * Material.bc * Defc1))
            If SigmaC > OldDefSigmaC Then
                OldDefSigmaC = SigmaC
            Else
                Material.bc -= 2 * bcStep
                OldDefSigmaC = Fc0 * ((1 + Material.ac) * Exp(-Material.bc * Defc1) - Material.ac * Exp(-2 * Material.bc * Defc1))
                bcStep = bcStep / 10
            End If
        Loop Until bcStep < Tol
        Material.bt = Material.bc * (Ft0 / Fc0) * (Gch / Gf) * ((1 + 0.5 * Material.at) / (1 + 0.5 * Material.ac))

    End Sub

    Public Sub ChargerStressStrainCurve()

        Dim StressVect(6), StrainVect(6), StrI, StrII, StrIII, SigmaI, SigmaII, SigmaIII As Double
        Dim I As Integer = 5
        StrainVect(1) = MoyenneTenseur(DefoX) ' DefoX(I) ' 
        StrainVect(2) = MoyenneTenseur(DefoY) ' DefoY(I) ' 
        StrainVect(3) = MoyenneTenseur(DefoZ) ' DefoZ(I) ' 
        StrainVect(4) = MoyenneTenseur(DefoYZ) ' DefoYZ(I) ' 
        StrainVect(5) = MoyenneTenseur(DefoZX) ' DefoZX(I) ' 
        StrainVect(6) = MoyenneTenseur(DefoXY) ' DefoXY(I) '  
        StressVect(1) = MoyenneTenseur(SigmaX) ' SigmaX(I) ' 
        StressVect(2) = MoyenneTenseur(SigmaY) ' SigmaY(I) ' 
        StressVect(3) = MoyenneTenseur(SigmaZ) ' SigmaZ(I) '  
        StressVect(4) = MoyenneTenseur(TouYZ) ' TouYZ(I) ' 
        StressVect(5) = MoyenneTenseur(TouZX) ' TouZX(I) ' 
        StressVect(6) = MoyenneTenseur(TouXY) ' TouXY(I) '
        Dim J As Integer
        ValeurPropre(StrainVect, StrI, StrII, StrIII)
        ValeurPropre(StressVect, SigmaI, SigmaII, SigmaIII)
        Dim r As Double = Rfunction(SigmaI, SigmaII, SigmaIII)
        Dim DefT As Double = Max(Max(StrI, StrII), StrIII) 'r * Max(Max(StrI, StrII), StrIII)
        Dim DefC As Double = -(1 - r) * Min(Min(StrI, StrII), StrIII)
        Dim SigmaT As Double = r * Max(Max(SigmaI, SigmaII), SigmaIII)
        Dim SigmaC As Double = -(1 - r) * Min(Min(SigmaI, SigmaII), SigmaIII)
        Dim ComprissivePoint As DoublePoint
        ComprissivePoint.y = SigmaC
        ComprissivePoint.x = DefC
        CompressiveStressStrain.Add(ComprissivePoint)

        Dim TensilePoint As DoublePoint
        TensilePoint.y = SigmaT
        TensilePoint.x = DefT
        TensileStressStrain.Add(TensilePoint)

    End Sub
    Public Sub ChargerLoadDisplacementCurve(Load As Double, Displacement As Double)
        Dim OurPoint As DoublePoint
        OurPoint.y = Abs(Load)
        OurPoint.x = Displacement
        LoadDisplacement.Add(OurPoint)
        Dim ValueParameter As Double = MoyenneTenseur(DamageParameter)
        ListOfDamageParameter.Add(ValueParameter)
    End Sub
    Public Function CalculLeq() As Double
        ' Ce calcul de volume est valide seulement dans le cas ou la heuteur de l'element est fixe
        Dim Surface1 As Double = SurfaceTriangle(ListNoeud.Item(0).Coord(1), ListNoeud.Item(0).Coord(2), ListNoeud.Item(1).Coord(1), ListNoeud.Item(1).Coord(2),
                                 ListNoeud.Item(2).Coord(1), ListNoeud.Item(2).Coord(2)) + SurfaceTriangle(ListNoeud.Item(0).Coord(1), ListNoeud.Item(0).Coord(2),
                                  ListNoeud.Item(2).Coord(1), ListNoeud.Item(2).Coord(2), ListNoeud.Item(3).Coord(1), ListNoeud.Item(3).Coord(2))

        Dim h As Double = Abs(ListNoeud.Item(0).Coord(2) - ListNoeud.Item(4).Coord(3))
        Dim Surface2 As Double = h * Max(Distance(ListNoeud.Item(3).Coord(1), ListNoeud.Item(3).Coord(2), ListNoeud.Item(0).Coord(1), ListNoeud.Item(0).Coord(2)),
                                         Max(Distance(ListNoeud.Item(3).Coord(1), ListNoeud.Item(3).Coord(2), ListNoeud.Item(2).Coord(1), ListNoeud.Item(2).Coord(2)),
                                         Max(Distance(ListNoeud.Item(0).Coord(1), ListNoeud.Item(0).Coord(2), ListNoeud.Item(1).Coord(1), ListNoeud.Item(1).Coord(2)),
                                         Distance(ListNoeud.Item(1).Coord(1), ListNoeud.Item(1).Coord(2), ListNoeud.Item(2).Coord(1), ListNoeud.Item(2).Coord(2)))))

        ' Dim Volume As double = Surface1 * h
        '  Dim LargestArea As double = Max(Surface2, Surface1)
        Return 1000 * (Surface1 * h) / Max(Surface2, Surface1)
    End Function


    Public Sub InitiStrain(Ngp As Integer)
        Dim igp As Integer
        For Mb = 1 To Ngp
            For Mc = 1 To Ngp
                For Mf = 1 To Ngp
                    igp += 1
                Next
            Next
        Next

        ReDim DefoX(igp)
        ReDim DefoY(igp)
        ReDim DefoZ(igp)
        ReDim DefoXY(igp)
        ReDim DefoYZ(igp)
        ReDim DefoZX(igp)
    End Sub

    Public Sub InitiStress(Ngp As Integer)
        Dim igp As Integer
        For Mb = 1 To Ngp
            For Mc = 1 To Ngp
                For Mf = 1 To Ngp
                    igp += 1
                Next
            Next
        Next
        ReDim DamageParameter(igp)
        ReDim SigmaX(igp)
        ReDim SigmaY(igp)
        ReDim SigmaZ(igp)
        ReDim TouXY(igp)
        ReDim TouYZ(igp)
        ReDim TouZX(igp)
    End Sub
    Public Sub ResoudreLinaireStress(Ngp As Integer, ELD() As Double, Samp(,) As Double)

        Dim EPS(6), SIGMA(6), x, Det As Double
        Dim igp As Integer


        For Mb = 1 To Ngp
            For Mc = 1 To Ngp
                For Mf = 1 To Ngp
                    'Calcul de deformation
                    igp += 1
                    Call Fmlin3(Mf, Mc, Mb, Samp)

                    'jacobien matrix
                    For ii = 1 To 3
                        For ji = 1 To 3
                            x = 0
                            For f = 1 To 8
                                x += DER(ii, f) * ListNoeud(f - 1).Coord(ji)
                            Next
                            JAC(ii, ji) = x
                        Next
                    Next
                    'Déterminant de la matrice jacobéenne

                    Det = JAC(1, 1) * DetM(JAC(2, 2), JAC(2, 3), JAC(3, 2), JAC(3, 3)) -
                          JAC(1, 2) * DetM(JAC(2, 1), JAC(2, 3), JAC(3, 1), JAC(3, 3)) +
                          JAC(1, 3) * DetM(JAC(2, 1), JAC(2, 2), JAC(3, 1), JAC(3, 2))

                    ' Det = AA * BB
                    'matrice inverse de la matrice jacobéenne
                    JAC1(1, 1) = JAC(2, 2) * JAC(3, 3) - JAC(3, 2) * JAC(2, 3)
                    JAC1(2, 1) = -JAC(2, 1) * JAC(3, 3) + JAC(3, 1) * JAC(2, 3)
                    JAC1(3, 1) = JAC(2, 1) * JAC(3, 2) - JAC(3, 1) * JAC(2, 2)
                    JAC1(1, 2) = -JAC(1, 2) * JAC(3, 3) + JAC(3, 2) * JAC(1, 3)
                    JAC1(2, 2) = JAC(1, 1) * JAC(3, 3) - JAC(3, 1) * JAC(1, 3)
                    JAC1(3, 2) = -JAC(1, 1) * JAC(3, 2) + JAC(3, 1) * JAC(1, 2)
                    JAC1(1, 3) = JAC(1, 2) * JAC(2, 3) - JAC(2, 2) * JAC(1, 3)
                    JAC1(2, 3) = -JAC(1, 1) * JAC(2, 3) + JAC(2, 1) * JAC(1, 3)
                    JAC1(3, 3) = JAC(1, 1) * JAC(2, 2) - JAC(2, 1) * JAC(1, 2)

                    For f = 1 To 3
                        For l = 1 To 3
                            JAC1(f, l) = JAC1(f, l) / Det
                        Next
                    Next
                    '--------------rest a verifié
                    For ii = 1 To 3
                        For ji = 1 To 8
                            x = 0
                            For f = 1 To 3
                                x += JAC1(ii, f) * DER(f, ji)
                            Next
                            DERIV(ii, ji) = x
                        Next
                    Next
                    For ij = 1 To 24
                        For ii = 1 To 6
                            BEE(ii, ij) = 0
                        Next
                    Next
                    Call FormB3()



                    For Md = 1 To 6
                        x = 0
                        For mg = 1 To 24
                            x += BEE(Md, mg) * ELD(mg)
                        Next
                        EPS(Md) = x
                    Next
                    For Md = 1 To 6
                        x = 0
                        For mi = 1 To 6
                            x += DEE(Md, mi) * EPS(mi)
                        Next
                        SIGMA(Md) = x
                    Next
                    'Contrainte
                    SigmaX(igp) = SIGMA(1)
                    SigmaY(igp) = SIGMA(2)
                    SigmaZ(igp) = SIGMA(3)
                    TouXY(igp) = SIGMA(4)
                    TouYZ(igp) = SIGMA(5)
                    TouZX(igp) = SIGMA(6)

                    DefoX(igp) = EPS(1)
                    DefoY(igp) = EPS(2)
                    DefoZ(igp) = EPS(3)
                    DefoXY(igp) = EPS(4)
                    DefoYZ(igp) = EPS(5)
                    DefoZX(igp) = EPS(6)
                Next
            Next
        Next
    End Sub


    Public Function LoadVector(Ngp As Integer, Rhoo As Double, Samp(,) As Double) As Double()
        Dim LoadVecteur(4) As Double
        Dim NTRhou(4), SIGMA(6), x, Det, QUOT As Double


        For I = 1 To Ngp
            For J = 1 To Ngp
                Call FmQuad(I, J, Samp)
                'jacobien matrix
                For ii = 1 To 2
                    For ji = 1 To 2
                        x = 0
                        For f = 1 To 4
                            x += DerQuad(ii, f) * ListNoeud(f + 3).Coord(ji)
                        Next
                        JACQuad(ii, ji) = x
                    Next
                Next
                'Déterminant de la matrice jacobéenne
                Det = JACQuad(1, 1) * JACQuad(2, 2) - JACQuad(1, 2) * JACQuad(2, 1)
                'matrice inverse de la matrice jacobéenne
                JACQuad1(1, 1) = JACQuad(2, 2)
                JACQuad1(1, 2) = -JACQuad(1, 2)
                JACQuad1(2, 1) = -JACQuad(2, 1)
                JACQuad1(2, 2) = JACQuad(1, 1)

                For f = 1 To 2
                    For l = 1 To 2
                        JACQuad1(f, l) = JACQuad1(f, l) / Det
                    Next
                Next

                For ii = 1 To 2
                    For ji = 1 To 4
                        x = 0
                        For f = 1 To 2
                            x += JACQuad1(ii, f) * DerQuad(f, ji)
                        Next
                        DerQuadIV(ii, ji) = x
                    Next
                Next

                QUOT = Det * Samp(I, 2) * Samp(J, 2)

                For ii = 1 To 4
                    NTRhou(ii) = FUNQuad(ii) * Rhoo * QUOT
                Next ii

                For ic = 1 To 4
                    LoadVecteur(ic) = LoadVecteur(ic) + NTRhou(ic)
                Next
            Next
        Next
        Return LoadVecteur
    End Function

    Public Sub ResoudreNonLinaireStressInitialStrain(Approch As Integer, Ngp As Integer, ELD() As Double, Optional LoiComportement As Integer = 0, Optional Convergance As Boolean = Nothing,
                       Optional Iter As Integer = 0, Optional MaxIter As Integer = 0, Optional Samp(,) As Double = Nothing, Optional DT As Double = 0)

        Dim ElasticStrain(6), SIGMA(6), StressVec(6), DEVP(6), Evp(6), x, THETA, Sigm As Double
        Dim igp As Integer
        Dim DSBAR, QUOT As Double
        Dim YieldFunction As Double
        Dim Erate(6) As Double

        Dim SigmaC, SigmaT, Dc, D As Double

        Dim SigmaI, SigmaII, SigmaIII, SigmaT0, VicoPlasticStrain(6), TotalStrain(6) As Double
        ReDim Bload(24)
        Dim ADefor, ADeforPlastic, Det As Double
        For Mb = 1 To Ngp
            For Mc = 1 To Ngp
                For Mf = 1 To Ngp
                    Call Fmlin3(Mf, Mc, Mb, Samp)

                    'jacobien matrix
                    For ii = 1 To 3
                        For ji = 1 To 3
                            x = 0
                            For f = 1 To 8
                                x += DER(ii, f) * ListNoeud(f - 1).Coord(ji)
                            Next
                            JAC(ii, ji) = x
                        Next
                    Next
                    'Déterminant de la matrice jacobéenne

                    Det = JAC(1, 1) * DetM(JAC(2, 2), JAC(2, 3), JAC(3, 2), JAC(3, 3)) -
                          JAC(1, 2) * DetM(JAC(2, 1), JAC(2, 3), JAC(3, 1), JAC(3, 3)) +
                          JAC(1, 3) * DetM(JAC(2, 1), JAC(2, 2), JAC(3, 1), JAC(3, 2))

                    ' Det = AA * BB
                    'matrice inverse de la matrice jacobéenne
                    JAC1(1, 1) = JAC(2, 2) * JAC(3, 3) - JAC(3, 2) * JAC(2, 3)
                    JAC1(2, 1) = -JAC(2, 1) * JAC(3, 3) + JAC(3, 1) * JAC(2, 3)
                    JAC1(3, 1) = JAC(2, 1) * JAC(3, 2) - JAC(3, 1) * JAC(2, 2)
                    JAC1(1, 2) = -JAC(1, 2) * JAC(3, 3) + JAC(3, 2) * JAC(1, 3)
                    JAC1(2, 2) = JAC(1, 1) * JAC(3, 3) - JAC(3, 1) * JAC(1, 3)
                    JAC1(3, 2) = -JAC(1, 1) * JAC(3, 2) + JAC(3, 1) * JAC(1, 2)
                    JAC1(1, 3) = JAC(1, 2) * JAC(2, 3) - JAC(2, 2) * JAC(1, 3)
                    JAC1(2, 3) = -JAC(1, 1) * JAC(2, 3) + JAC(2, 1) * JAC(1, 3)
                    JAC1(3, 3) = JAC(1, 1) * JAC(2, 2) - JAC(2, 1) * JAC(1, 2)

                    For f = 1 To 3
                        For l = 1 To 3
                            JAC1(f, l) = JAC1(f, l) / Det
                        Next
                    Next
                    '--------------rest a verifié
                    For ii = 1 To 3
                        For ji = 1 To 8
                            x = 0
                            For f = 1 To 3
                                x += JAC1(ii, f) * DER(f, ji)
                            Next
                            DERIV(ii, ji) = x
                        Next
                    Next
                    For ij = 1 To 24
                        For ii = 1 To 6
                            BEE(ii, ij) = 0
                        Next
                    Next
                    Call FormB3()
                    For il = 1 To 6
                        For jl = 1 To 24
                            BT(jl, il) = BEE(il, jl)
                        Next jl
                    Next il

                    igp += 1
                    VicoPlasticStrain(1) = DefoX(igp)
                    VicoPlasticStrain(2) = DefoY(igp)
                    VicoPlasticStrain(3) = DefoZ(igp)
                    VicoPlasticStrain(4) = DefoXY(igp)
                    VicoPlasticStrain(5) = DefoYZ(igp)
                    VicoPlasticStrain(6) = DefoZX(igp)
                    'VicoPlasticStrain matrix


                    'Calcul de deformation
                    TotalStrain = MatriceVecteurMultipe(BEE, ELD)

                    ADefor = NormeVecteur(TotalStrain)
                    For i = 1 To 6
                        ElasticStrain(i) = TotalStrain(i) - VicoPlasticStrain(i)
                    Next

                    SIGMA = MatriceVecteurMultipe(DEE, ElasticStrain)


                    StressVec(1) = SIGMA(1) + SigmaX(igp)
                    StressVec(2) = SIGMA(2) + SigmaY(igp)
                    StressVec(3) = SIGMA(3) + SigmaZ(igp)
                    StressVec(4) = SIGMA(4) + TouYZ(igp)
                    StressVec(5) = SIGMA(5) + TouZX(igp)
                    StressVec(6) = SIGMA(6) + TouXY(igp)


                    'Invar3D(StressVec, Sigm, DSBAR, THETA)
                    ValeurPropre(StressVec, SigmaI, SigmaII, SigmaIII, THETA, DSBAR, Sigm)

                    YieldFunction = YieldFunctionEstimation(LoiComportement, Approch, TotalStrain, StressVec, SigmaI, SigmaII, SigmaIII, Material.Fck, Material.Leq, Material.bc, Material.bt, Material.Fb0_Fc0, Material.Kc, Comprission, Tension, SigmaC, SigmaT, SigmaT0, Dc, DT, D, Material.E0)

                    If Convergance Or Iter = MaxIter Then
                        'Copier vecteur stress dans devp
                        For i = 1 To 6
                            DEVP(i) = StressVec(i)
                        Next
                        Eload = MatriceVecteurMultipe(BT, DEVP)
                        QUOT = Det * Samp(Mb, 2) * Samp(Mc, 2) * Samp(Mf, 2)
                        Bload = VecAdd(Bload, ScaleVectorMultipe(QUOT, Eload, 1))
                    Else
                        If YieldFunction >= 0 Then
                            Erate = DerivativePotentialFunction(LoiComportement, StressVec, Material.PsiDegre, Material.Excent, SigmaT0, YieldFunction, 0)
                            ' MsgBox(Erate(1) & "  " & Erate(2) & "  " & Erate(3) & "  " & Erate(4) & "  " & Erate(5) & "  " & Erate(6))
                            ADeforPlastic = NormeVecteur(Erate)
                            '
                            DT = StepTime(LoiComportement, Approch, 1, DEE, Material.PsiDegre, Material.Excent, TotalStrain, StressVec, Material.Fck, Material.Leq, Material.bc, Material.bt, Material.Fb0_Fc0, Material.Kc, Comprission, Tension, Material.E0)
                            ' DT = 0.01 * ADefor / ADeforPlastic
                            Evp = ScaleVectorMultipe(DT, Erate)
                            VicoPlasticStrain = VecAdd(VicoPlasticStrain, Evp)

                            DefoX(igp) = VicoPlasticStrain(1)
                            DefoY(igp) = VicoPlasticStrain(2)
                            DefoZ(igp) = VicoPlasticStrain(3)
                            DefoXY(igp) = VicoPlasticStrain(4)
                            DefoYZ(igp) = VicoPlasticStrain(5)
                            DefoZX(igp) = VicoPlasticStrain(6)

                            DEVP = MatriceVecteurMultipe(DEE, Evp, 1)
                            Eload = MatriceVecteurMultipe(BT, DEVP)
                            QUOT = Det * Samp(Mb, 2) * Samp(Mc, 2) * Samp(Mf, 2)
                            Bload = VecAdd(Bload, ScaleVectorMultipe(QUOT, Eload, 1))
                        End If
                    End If

                    If Convergance Or Iter = MaxIter Then
                        SigmaX(igp) = StressVec(1)
                        SigmaY(igp) = StressVec(2)
                        SigmaZ(igp) = StressVec(3)
                        TouYZ(igp) = StressVec(4)
                        TouZX(igp) = StressVec(5)
                        TouXY(igp) = StressVec(6)
                    End If

                Next
            Next
        Next



    End Sub
    Public Sub ResoudreNonLinaireStressInitialStress(ChergeTotale As Double, Approch As Integer, Ngp As Integer, ELD() As Double, Optional LoiComportement As Integer = 0, Optional ByRef Convergance As Boolean = Nothing,
                       Optional Iter As Integer = 0, Optional MaxIter As Integer = 0, Optional Samp(,) As Double = Nothing)

        Dim EPS(6), NewEPS(6), OldEPS(6), NewEPS1(6), OldEPS1(6), SIGMA(6), OldStress(6), NewStress(6),
           CurrentStress(6), NewStress1(6), OldStress1(6), DEVP(6), Evp(6), x, THETA,
            Sigm As Double
        Dim igp As Integer
        Dim DSBAR, QUOT As Double
        Dim NewF, NewF1, OldF1, Alfa, DInelasticStrain As Double
        Dim Erate(6) As Double
        Dim ELSO(6) As Double
        Array.Clear(Bload, 0, Bload.Count)

        For Mb = 1 To Ngp
            For Mc = 1 To Ngp
                For Mf = 1 To Ngp
                    Dim SigmaC, SigmaT, Dc, Dt, PlasticStress(6), D As Double
                    Dim SigmaI, SigmaII, SigmaIII, SigmaT0, Evpt(6), Det As Double
                    igp += 1
                    Call Fmlin3(Mf, Mc, Mb, Samp)

                    'jacobien matrix
                    For ii = 1 To 3
                        For ji = 1 To 3
                            x = 0
                            For f = 1 To 8
                                x += DER(ii, f) * ListNoeud(f - 1).Coord(ji)
                            Next
                            JAC(ii, ji) = x
                        Next
                    Next
                    'Déterminant de la matrice jacobéenne

                    Det = JAC(1, 1) * DetM(JAC(2, 2), JAC(2, 3), JAC(3, 2), JAC(3, 3)) -
                          JAC(1, 2) * DetM(JAC(2, 1), JAC(2, 3), JAC(3, 1), JAC(3, 3)) +
                          JAC(1, 3) * DetM(JAC(2, 1), JAC(2, 2), JAC(3, 1), JAC(3, 2))

                    ' Det = AA * BB
                    'matrice inverse de la matrice jacobéenne
                    JAC1(1, 1) = JAC(2, 2) * JAC(3, 3) - JAC(3, 2) * JAC(2, 3)
                    JAC1(2, 1) = -JAC(2, 1) * JAC(3, 3) + JAC(3, 1) * JAC(2, 3)
                    JAC1(3, 1) = JAC(2, 1) * JAC(3, 2) - JAC(3, 1) * JAC(2, 2)
                    JAC1(1, 2) = -JAC(1, 2) * JAC(3, 3) + JAC(3, 2) * JAC(1, 3)
                    JAC1(2, 2) = JAC(1, 1) * JAC(3, 3) - JAC(3, 1) * JAC(1, 3)
                    JAC1(3, 2) = -JAC(1, 1) * JAC(3, 2) + JAC(3, 1) * JAC(1, 2)
                    JAC1(1, 3) = JAC(1, 2) * JAC(2, 3) - JAC(2, 2) * JAC(1, 3)
                    JAC1(2, 3) = -JAC(1, 1) * JAC(2, 3) + JAC(2, 1) * JAC(1, 3)
                    JAC1(3, 3) = JAC(1, 1) * JAC(2, 2) - JAC(2, 1) * JAC(1, 2)

                    For f = 1 To 3
                        For l = 1 To 3
                            JAC1(f, l) = JAC1(f, l) / Det
                        Next
                    Next
                    '--------------rest a verifié
                    For ii = 1 To 3
                        For ji = 1 To 8
                            x = 0
                            For f = 1 To 3
                                x += JAC1(ii, f) * DER(f, ji)
                            Next
                            DERIV(ii, ji) = x
                        Next
                    Next
                    For ij = 1 To 24
                        For ii = 1 To 6
                            BEE(ii, ij) = 0
                        Next
                    Next
                    Call FormB3()
                    For il = 1 To 6
                        For jl = 1 To 24
                            BT(jl, il) = BEE(il, jl)
                        Next jl
                    Next il

                    ReDim ELSO(6)

                    'Calcul de deformation

                    For Md = 1 To 6
                        x = 0
                        For mg = 1 To 24
                            x += BEE(Md, mg) * ELD(mg)
                        Next
                        EPS(Md) = x
                    Next

                    For Md = 1 To 6
                        x = 0
                        For mi = 1 To 6
                            x += DEE(Md, mi) * EPS(mi)
                        Next
                        SIGMA(Md) = x
                    Next



                    ' Cherger les nouvelle valeurs des déformations
                    NewEPS(1) = DefoX(igp) + EPS(1)
                    NewEPS(2) = DefoY(igp) + EPS(2)
                    NewEPS(3) = DefoZ(igp) + EPS(3)
                    NewEPS(4) = DefoYZ(igp) + EPS(4)
                    NewEPS(5) = DefoZX(igp) + EPS(5)
                    NewEPS(6) = DefoXY(igp) + EPS(6)

                    ' Cherger les anciennes valeurs des déformations
                    OldEPS(1) = DefoX(igp)
                    OldEPS(2) = DefoY(igp)
                    OldEPS(3) = DefoZ(igp)
                    OldEPS(4) = DefoYZ(igp)
                    OldEPS(5) = DefoZX(igp)
                    OldEPS(6) = DefoXY(igp)
                    ' Cherger les anciennes valeurs des contraintes
                    OldStress(1) = SigmaX(igp) / (1 - DamageParameter(igp))
                    OldStress(2) = SigmaY(igp) / (1 - DamageParameter(igp))
                    OldStress(3) = SigmaZ(igp) / (1 - DamageParameter(igp))
                    OldStress(4) = TouYZ(igp) / (1 - DamageParameter(igp))
                    OldStress(5) = TouZX(igp) / (1 - DamageParameter(igp))
                    OldStress(6) = TouXY(igp) / (1 - DamageParameter(igp))
                    ' Cherger les nouvelles valeurs des contraintes
                    NewStress = VecAdd(SIGMA, OldStress)
                    'Calculer la valeur de F(NewStress)
                    ValeurPropre(NewStress, SigmaI, SigmaII, SigmaIII, THETA, DSBAR, Sigm)
                    NewF = YieldFunctionEstimation(LoiComportement, Approch, NewEPS, NewStress, SigmaI, SigmaII, SigmaIII, Material.Fck, Material.Leq, Material.bc, Material.bt, Material.Fb0_Fc0, Material.Kc, Comprission, Tension, SigmaC, SigmaT, SigmaT0, Dc, Dt, D, Material.E0)

                    If NewF >= 0 Then
                        NewF1 = NewF
                        'Cas : Plastique
                        '1- Calculer Alfa
                        Dim CalculateAuto As Boolean = False
                        Dim NewAlfa, OldAlfa As Double

                        Alfa = 1
                        OldAlfa = 0
                        If CalculateAuto Then
                            Do
                                OldStress1 = VecAdd(ScaleVectorMultipe(OldAlfa, SIGMA), OldStress)
                                ValeurPropre(OldStress1, SigmaI, SigmaII, SigmaIII, THETA, DSBAR, Sigm)
                                OldEPS1 = VecAdd(ScaleVectorMultipe(OldAlfa, EPS), OldEPS)
                                OldF1 = YieldFunctionEstimation(LoiComportement, Approch, OldEPS1, OldStress1, SigmaI, SigmaII, SigmaIII, Material.Fck, Material.Leq, Material.bc, Material.bt, Material.Fb0_Fc0, Material.Kc, Comprission, Tension, SigmaC, SigmaT, SigmaT0, Dc, Dt, D, Material.E0)
                                NewStress1 = VecAdd(ScaleVectorMultipe(Alfa, SIGMA), OldStress)
                                NewEPS1 = VecAdd(ScaleVectorMultipe(Alfa, EPS), OldEPS)
                                ValeurPropre(NewStress1, SigmaI, SigmaII, SigmaIII, THETA, DSBAR, Sigm)
                                NewF1 = YieldFunctionEstimation(LoiComportement, Approch, NewEPS1, NewStress1, SigmaI, SigmaII, SigmaIII, Material.Fck, Material.Leq, Material.bc, Material.bt, Material.Fb0_Fc0, Material.Kc, Comprission, Tension, SigmaC, SigmaT, SigmaT0, Dc, Dt, D, Material.E0)
                                NewAlfa = Alfa - (Alfa - OldAlfa) * NewF1 / (NewF1 - OldF1)
                                If Abs(NewAlfa - Alfa) < 0.001 Then
                                    Alfa = 1 - Alfa
                                    Exit Do
                                Else
                                    OldAlfa = Alfa
                                    Alfa = NewAlfa
                                End If
                            Loop
                        Else
                            ' MsgBox(OldStress(3) & "  " & OldEPS(3))
                            ValeurPropre(OldStress, SigmaI, SigmaII, SigmaIII, THETA, DSBAR, Sigm)
                            OldAlfa = YieldFunctionEstimation(LoiComportement, Approch, OldEPS, OldStress, SigmaI, SigmaII, SigmaIII, Material.Fck, Material.Leq, Material.bc, Material.bt, Material.Fb0_Fc0, Material.Kc, Comprission, Tension, SigmaC, SigmaT, SigmaT0, Dc, Dt, D, Material.E0)
                            Alfa = NewF / (NewF - OldAlfa)
                        End If
                        Dim EulerIntegration As Boolean = False
                        If EulerIntegration Then
                            Dim InelasticStrain As Double
                            NewEPS1 = VecAdd(EPS, OldEPS)
                            NewStress = VecAdd(SIGMA, OldStress)
                            ValeurPropre(NewStress, SigmaI, SigmaII, SigmaIII, THETA, DSBAR, Sigm)
                            Dim SigmaMax As Double = SigmaMax = Max(Max(SigmaI, SigmaII), SigmaIII)
                            YieldFunctionEstimation(LoiComportement, Approch, NewEPS1, NewStress, SigmaI, SigmaII, SigmaIII,
                                                    Material.Fck, Material.Leq, Material.bc, Material.bt, Material.Fb0_Fc0,
                                                    Material.Kc, Comprission, Tension, SigmaC, SigmaT, SigmaT0, Dc, Dt, D,
                                                    Material.E0, InelasticStrain)

                            PlasticStress = Material.ModifiedEulerSchemeWithSubstepping(Approch, OldStress, SIGMA, Alfa, EPS, OldEPS, DEE,
                                                                                        Comprission, Tension, InelasticStrain, SigmaMax, SigmaT, 0.001, 0.01)
                            '  EStress = VecAdd(VecAdd(OldStress, SIGMA), NewStress, -1)
                        Else
                            '------ With Implicite method ----------
                            'Dim F1, F2 As Double
                            '  F1 = NewF
                            ' Dim InelasticStrain As Double
                            'ValeurPropre(OldStress, SigmaI, SigmaII, SigmaIII, THETA, DSBAR, Sigm)
                            'YieldFunctionEstimation(LoiComportement, Approch, OldEPS, OldStress, SigmaI, SigmaII, SigmaIII,
                            'Material.Fck, Material.Leq, Material.bc, Material.bt, Material.Fb0_Fc0,
                            'Material.Kc, Comprission, Tension, SigmaC, SigmaT, SigmaT0, Dc, Dt, D,
                            'Material.PhiDegre, Material.Cohesion, Material.E0, DInelasticStrain)
                            'NewEPS1 = VecAdd(EPS, OldEPS)
                            'NewStress = VecAdd(SIGMA, OldStress)
                            'ValeurPropre(NewStress, SigmaI, SigmaII, SigmaIII, THETA, DSBAR, Sigm)
                            'YieldFunctionEstimation(LoiComportement, Approch, NewEPS1, NewStress, SigmaI, SigmaII, SigmaIII,
                            ' Material.Fck, Material.Leq, Material.bc, Material.bt, Material.Fb0_Fc0,
                            'Material.Kc, Comprission, Tension, SigmaC, SigmaT, SigmaT0, Dc, Dt, D,
                            'Material.PhiDegre, Material.Cohesion, Material.E0, InelasticStrain)

                            'DInelasticStrain = InelasticStrain - DInelasticStrain
                            'PlasticStress = Material.PlasticStress(Approch, NewF, NewEPS1, NewStress, DEE, EPS, InelasticStrain, DInelasticStrain, SigmaT, Comprission, Tension)
                            'NewStress = VecAdd(NewStress, PlasticStress, -1)
                            'ValeurPropre(NewStress, SigmaI, SigmaII, SigmaIII, THETA, DSBAR, Sigm)
                            'NewF = YieldFunctionEstimation(LoiComportement, Approch, NewEPS1, NewStress, SigmaI, SigmaII, SigmaIII, Material.Fck, Material.Leq, Material.bc, Material.bt, Material.Fb0_Fc0, Material.Kc, Comprission, Tension, SigmaC, SigmaT, SigmaT0, Dc, Dt, D, Material.PhiDegre, Material.Cohesion, Material.E0, InelasticStrain)
                            'F2 = NewF
                            'If NewF > 0.01 Then
                            ' PlasticStress = Material.CorrectPlasticStress(0.01, Approch, NewEPS1, NewStress, DEE, EPS, InelasticStrain, DInelasticStrain, SigmaT, Comprission, Tension, SIGMA, OldStress)
                            ' End If

                            '------
                            ' Alfa = 1 - Alfa

                            Dim InelasticStrain As Double
                            ValeurPropre(OldStress, SigmaI, SigmaII, SigmaIII, THETA, DSBAR, Sigm)
                            YieldFunctionEstimation(LoiComportement, Approch, OldEPS, OldStress, SigmaI, SigmaII, SigmaIII,
                                                    Material.Fck, Material.Leq, Material.bc, Material.bt, Material.Fb0_Fc0,
                                                    Material.Kc, Comprission, Tension, SigmaC, SigmaT, SigmaT0, Dc, Dt, D,
                                                    Material.E0, DInelasticStrain)


                            NewEPS1 = VecAdd(EPS, OldEPS)
                            NewStress = VecAdd(SIGMA, OldStress)
                            ValeurPropre(NewStress, SigmaI, SigmaII, SigmaIII, THETA, DSBAR, Sigm)
                            Dim SigmaMax As Double = Max(SigmaIII, Max(SigmaI, SigmaII))
                            YieldFunctionEstimation(LoiComportement, Approch, NewEPS1, NewStress, SigmaI, SigmaII, SigmaIII,
                            Material.Fck, Material.Leq, Material.bc, Material.bt, Material.Fb0_Fc0,
                            Material.Kc, Comprission, Tension, SigmaC, SigmaT, SigmaT0, Dc, Dt, D,
                            Material.E0, InelasticStrain)
                            DInelasticStrain = Abs(InelasticStrain - DInelasticStrain)
                            PlasticStress = Material.PlasticStressImplicit(NewEPS1, NewStress, DEE, EPS, InelasticStrain, DInelasticStrain, SigmaT, SigmaMax, Alfa)


                            '  PlasticStress = ScaleVectorMultipe(Alfa, SIGMA)
                        End If
                        Dim Eload As Double()
                        'Problem  EStress  DP
                        Eload = MatriceVecteurMultipe(BT, PlasticStress, 1)
                        QUOT = Det * Samp(Mb, 2) * Samp(Mc, 2) * Samp(Mf, 2)
                        Bload = VecAdd(Bload, ScaleVectorMultipe(QUOT, Eload, 1))

                        DamageParameter(igp) = D
                        SigmaX(igp) = (OldStress(1) + SIGMA(1) - PlasticStress(1)) * (1 - D)
                        SigmaY(igp) = (OldStress(2) + SIGMA(2) - PlasticStress(2)) * (1 - D)
                        SigmaZ(igp) = (OldStress(3) + SIGMA(3) - PlasticStress(3)) * (1 - D)
                        TouYZ(igp) = (OldStress(4) + SIGMA(4) - PlasticStress(4)) * (1 - D)
                        TouZX(igp) = (OldStress(5) + SIGMA(5) - PlasticStress(5)) * (1 - D)
                        TouXY(igp) = (OldStress(6) + SIGMA(6) - PlasticStress(6)) * (1 - D)

                        DefoX(igp) += EPS(1)
                        DefoY(igp) += EPS(2)
                        DefoZ(igp) += EPS(3)
                        DefoYZ(igp) += EPS(4)
                        DefoZX(igp) += EPS(5)
                        DefoXY(igp) += EPS(6)

                    Else
                        SigmaX(igp) = (OldStress(1) + SIGMA(1)) * (1 - D)
                        SigmaY(igp) = (OldStress(2) + SIGMA(2)) * (1 - D)
                        SigmaZ(igp) = (OldStress(3) + SIGMA(3)) * (1 - D)
                        TouYZ(igp) = (OldStress(4) + SIGMA(4)) * (1 - D)
                        TouZX(igp) = (OldStress(5) + SIGMA(5)) * (1 - D)
                        TouXY(igp) = (OldStress(6) + SIGMA(6)) * (1 - D)
                        DefoX(igp) += EPS(1)
                        DefoY(igp) += EPS(2)
                        DefoZ(igp) += EPS(3)
                        DefoYZ(igp) += EPS(4)
                        DefoZX(igp) += EPS(5)
                        DefoXY(igp) += EPS(6)
                    End If




                Next
            Next
        Next



    End Sub

    Public Function YieldFunctionEstimation(LoiComportement As Integer, Approch As Integer, Strain() As Double, Stress() As Double,
                                           SigmaI As Double, SigmaII As Double, SigmaIII As Double, Fck As Double, Ieq As Double, bcBakhti As Double, btBakhti As Double, Fb0_Fc0 As Double,
                                           Kc As Double, Comprission As List(Of DoublePoint), Tension As List(Of DoublePoint),
                                            ByRef SigmaC As Double, ByRef SigmaT As Double, ByRef SigmaT0 As Double,
                                            ByRef Dc As Double, ByRef Dt As Double, ByRef D As Double,
                                            Optional Ebeton As Double = 0, ByRef Optional DefPlasC As Double = 0) As Double
        Dim YieldFunction As Double = 0
        Select Case LoiComportement
            Case 0 'Concrete Damaged Plasticity Model: J.Lubliner 1989
                'Damaged Paramaters Evaluation
                Dim DefPlasT As Double
                Dim Resultats As Boolean = Material.DamageParametres(Approch, Strain, SigmaI, SigmaII, SigmaIII, Material.Fck, Ieq, bcBakhti, btBakhti,
                                                            SigmaC, SigmaT, SigmaT0, Dc, Dt, D, Comprission, Tension, Material.E0, DefPlasT, DefPlasC)
                If Dc = 1 Then Dc = 0.999999999
                If Dt = 1 Then Dt = 0.999999999
                '            If Max(SigmaI, Max(SigmaII, SigmaIII)) > 35 Then MsgBox(Max(SigmaI, Max(SigmaII, SigmaIII)))
                '            If Min(SigmaI, Min(SigmaII, SigmaIII)) < -35 Then MsgBox(Min(SigmaI, Min(SigmaII, SigmaIII)))
                SigmaC = SigmaC / (1 - Dc)
                SigmaT = SigmaT / (1 - Dt)



                YieldFunction = Material.ConcreteDamagedPlasticityYieldCondition(Stress, SigmaI,
                                                                                 SigmaII, SigmaIII, Fb0_Fc0,
                                                                                 SigmaC, SigmaT, Kc)

            Case 1 'Mohr coulomb 
                'Damaged Paramaters Evaluation
                ' Dim Theta, Sigm, SigmaBare As Double
                '  ValeurPropre(Stress, SigmaI, SigmaII, SigmaIII, Theta, SigmaBare, Sigm)
                '  YieldFunction = MohrCoulomb(Phi, Theta, SigmaBare, Sigm, Cohesion)
        End Select
        Return YieldFunction
    End Function
    Public Function DerivativeFaillerFunctionToStrain(LoiComportement As Integer, InElasticStrain As Double, SigmaTbarre As Double, fc0 As Double,
                                                       SigmaMax As Double) As Double
        Dim Erate As Double
        Select Case LoiComportement
            Case 0 'Concrete Damaged Plasticity
                Erate = Material.DerivativeYieldFunctionStrain(InElasticStrain, fc0, SigmaTbarre, SigmaMax)
        End Select

        Return Erate

    End Function
    Public Function Dplastic(LoiComportement As Integer, Approch As Integer, Delastic(,) As Double, PsiDegre As Double, Excent As Double, Strain() As Double,
                              Stress() As Double, Fck As Double, Ieq As Double, bcBakhti As Double, btBakhti As Double, Fb0_Fc0 As Double, Kc As Double, Comprission As List(Of DoublePoint), Tension As List(Of DoublePoint),
                             Optional phi As Double = 0, Optional psi As Double = 0, Optional Ebeton As Double = 0) As Double(,)
        Select Case LoiComportement
            Case 0 'Concrete Damaged Plasticity
                Return Material.DplasticConcreteDamagedPlacticity(Approch, Delastic, PsiDegre, Excent, Strain, Stress, Fck, Ieq, bcBakhti, btBakhti, Fb0_Fc0, Kc, Comprission, Tension, Ebeton)
            Case 1 'Mohr-Coulomb
                ' Return DplasticMohrCoulomb(phi, psi, Delastic, Stress)
        End Select

        Return Nothing
    End Function
    Public Function ModifiedEulerIntegrationNewStressOutput(LoiComportement As Integer, Approch As Integer, OldStress() As Double, SIGMA() As Double,
                                                            EPS() As Double, Alfa As Double, SSTOL As Double, YTOL As Double,
                                                            OldEPS() As Double) As Double()

        Dim StrainS() As Double = ScaleVectorMultipe(1 - Alfa, EPS)
        Dim SigmaC, SigmaT, SigmaT0, Dc, DTens, D As Double
        Dim NewEPS() As Double = VecAdd(ScaleVectorMultipe(Alfa, EPS), OldEPS)
        Dim ElasticStress() As Double = ScaleVectorMultipe(Alfa, SIGMA)
        Dim Dp(,), Dep(,), DT, NewStress(), SigmaI, SigmaII, SigmaIII As Double
        Dim Stress1(6), Time, Stress2(6), EVect(), R, B, DStressTemp() As Double
        '   Dim SigmaI, SigmaII, SigmaIII, SigmaT0 As double
        '   Dim SigmaC, SigmaT, DamageC, DamageT, D As double
        DT = 1
        NewStress = VecAdd(OldStress, ElasticStress)
        Do While Time < 1
            Dim StrainSS(6) As Double
            For i = 1 To 6
                StrainSS(i) = StrainS(i) * DT
            Next

            '------------------------------------
            '  Dim InelasticStrain As Double
            '   NewStress = VecAdd(ScaleVectorMultipe(1 - Alfa, SIGMA), OldStress)
            'Calculer DP
            ' ValeurPropre(NewStress, SigmaI, SigmaII, SigmaIII)
            ' Dim SigmaMax As Double = Max(Max(SigmaI, SigmaII), SigmaIII)
            ' YieldFunctionEstimation(LoiComportement, Approch, NewEPS, NewStress, SigmaI, SigmaII, SigmaIII,
            '   Material.Fck, Material.Leq, Material.bc, Material.bt, Material.Fb0_Fc0,
            '                                          Material.Kc, Comprission, Tension, SigmaC, SigmaT, SigmaT0, Dc, DTens, D,
            '  Material.PhiDegre, Material.Cohesion, Material.E0, InelasticStrain)
            '  Stress1 = Material.PlasticStress(NewEPS, NewStress, DEE, EPS, InelasticStrain, SigmaT, SigmaMax, Alfa)
            '  Stress1 = VecAdd(SIGMA, Stress1, -1)
            '-----------------------------

            Dp = Dplastic(LoiComportement, Approch, DEE, Material.PsiDegre, Material.Excent, EPS, NewStress, Material.Fck, Material.Leq, Material.bc, Material.bt, Material.Fb0_Fc0, Material.Kc, Comprission, Tension,,, Material.E0)
            Dep = MatriceMatriceAdd(DEE, Dp, -1, 1)
            Stress1 = MatriceVecteurMultipe(Dep, StrainSS, 1)

            '      ValeurPropre(VecAdd(NewStress, Stress1), SigmaI, SigmaII, SigmaIII)
            '     SigmaMax = Max(Max(SigmaI, SigmaII), SigmaIII)
            '    YieldFunctionEstimation(LoiComportement, Approch, NewEPS, VecAdd(NewStress, Stress1), SigmaI, SigmaII, SigmaIII,
            ' Material.Fck, Material.Leq, Material.bc, Material.bt, Material.Fb0_Fc0,
            'Material.Kc, Comprission, Tension, SigmaC, SigmaT, SigmaT0, Dc, DTens, D,
            ' Material.PhiDegre, Material.Cohesion, Material.E0, InelasticStrain)
            '  Stress2 = Material.PlasticStress(NewEPS, VecAdd(NewStress, Stress1), DEE, EPS, InelasticStrain, SigmaT, SigmaMax, Alfa)
            '  Stress2 = VecAdd(SIGMA, Stress1, -1)

            Dp = Dplastic(LoiComportement, Approch, DEE, Material.PsiDegre, Material.Excent, EPS, VecAdd(NewStress, Stress1), Material.Fck, Material.Leq, Material.bc, Material.bt, Material.Fb0_Fc0, Material.Kc, Comprission, Tension,,, Material.E0)
            Dep = MatriceMatriceAdd(DEE, Dp, -1, 1)
            Stress2 = MatriceVecteurMultipe(Dep, StrainSS, 1)

            DStressTemp = ScaleVectorMultipe(0.5, VecAdd(Stress2, Stress1), 1)
            EVect = ScaleVectorMultipe(0.5, VecAdd(Stress2, ScaleVectorMultipe(-1, Stress1)), 1)
            R = NormeVecteur(EVect) / NormeVecteur(VecAdd(NewStress, DStressTemp))

            If R <= SSTOL Then
                Time += DT
                B = Max(Min(0.8 * Sqrt(SSTOL / R), 2), 0.1)
                DT = B * DT
                DT = Min(DT, 1 - Time)
                NewStress = VecAdd(NewStress, DStressTemp)
                Exit Do
            Else
                B = Max(Min(0.8 * Sqrt(SSTOL / R), 2), 0.1)
                DT = B * DT
            End If

            '  ValeurPropre(NewStress, SigmaI, SigmaII, SigmaIII)
            ' NewF = YieldFunctionEstimation(LoiComportement, Approch, NewEPS, NewStress, SigmaI, SigmaII, SigmaIII, Fck, Leq, BcBakhti, BtBakhti, Fb0_Fc0, Kc, Comprission, Tension, SigmaC, SigmaT, SigmaT0, DamageC, DamageT, D, PhiDegre, Cohesion, E0)
        Loop
        Return NewStress
    End Function

    Public Function Lamda(LoiComportement As Integer, PsiDegre As Double, Excent As Double, SigmaT0 As Double, SigmaC As Double, SigmaTBarre As Double, Strain() As Double,
                             Stress() As Double, Delastic(,) As Double, Alfa As Double, a As Double, b As Double, Gamma As Double, Theta As Double, SigmaI As Double,
                             ByRef DerivativeF() As Double, InElasticStrain As Double, DInElasticStrain As Double, SigmaMax As Double) As Double
        Select Case LoiComportement
            Case 0 'Concrete Damaged Plasticity
                Dim I1, J2, J3 As Double
                InvariantsContraintes(Stress, I1,,, J2, J3)
                If SigmaI < 0 Then
                    b = -Gamma
                End If
                DerivativeF = DerivativeFaillerFunction(LoiComportement, Stress, a, b, Theta, I1, J2, J3)
                Dim DerivativeQ() As Double = DerivativePotentialFunction(LoiComportement, Stress, PsiDegre, Excent, SigmaT0)
                Dim Result As Double = Material.DLambda(Strain, DerivativeQ, DerivativeF, Delastic, InElasticStrain, SigmaTBarre, SigmaMax, DInElasticStrain)
                Return Result / Alfa
        End Select
        Return Nothing
    End Function

    Public Sub Stress(ChergeTotale As Double, Approch As Integer, Ngp As Integer, ELD() As Double, Optional Comportement As Integer = 0, Optional LoiComportement As Integer = 0, Optional Convergance As Boolean = Nothing,
                       Optional Iter As Integer = 0, Optional MaxIter As Integer = 0, Optional Samp(,) As Double = Nothing, Optional StepTime As Double = 0)


        ' Comportement  0 : linéaire  1 : NonLinéaire
        If Comportement = 0 Then  ' Comportement linéaire
            ResoudreLinaireStress(Ngp, ELD, Samp)
        ElseIf Comportement = 1 Then  ' Comportement NonLinéaire
            ResoudreNonLinaireStressInitialStrain(Approch, Ngp, ELD, LoiComportement, Convergance, Iter, MaxIter, Samp, StepTime)
        ElseIf Comportement = 2 Then
            ResoudreNonLinaireStressInitialStress(ChergeTotale, Approch, Ngp, ELD, LoiComportement, Convergance, Iter, MaxIter, Samp)
        End If

    End Sub


    Public Sub Invar3D(Stress() As Double, ByRef Sigm As Double, ByRef DSBAR As Double, ByRef THETA As Double)
        Dim SQ3 As Double = Sqrt(3)
        Dim S1 As Double = Stress(1)
        Dim S2 As Double = Stress(2)
        Dim S3 As Double = Stress(3)
        Dim S4 As Double = Stress(4)
        Dim S5 As Double = Stress(5)
        Dim S6 As Double = Stress(6)
        Sigm = (S1 + S2 + S3) / 3
        Dim D2 As Double = ((S1 - S2) ^ 2 + (S2 - S3) ^ 2 + (S3 - S1) ^ 2) / 6 + S4 * S4 + S5 * S5 + S6 * S6
        Dim DS1 As Double = S1 - Sigm
        Dim DS2 As Double = S2 - Sigm
        Dim DS3 As Double = S3 - Sigm
        Dim D3 As Double = DS1 * DS2 * DS3 - DS1 * S5 * S5 - DS2 * S6 * S6 - DS3 * S4 * S4 + 2 * S4 * S5 * S6
        DSBAR = Sqrt(3) * Sqrt(D2)
        If DSBAR = 0 Then
            THETA = 0
        Else
            Dim SINE As Double = -3 * Sqrt(3) * D3 / (2 * Sqrt(D2) ^ 3)
            If SINE > 1 Then SINE = 1
            If SINE < -1 Then SINE = -1
            THETA = Asin(SINE) / 3
        End If
    End Sub
    Public Function MtriceRigiditeElementaire(Ngp As Integer, IndexNoued As List(Of Integer), Samp(,) As Double) As Double(,)

        Dim DBEE(6, 24), BTDB(24, 24), ecm1(24, 24), X, QUOT As Double

        Dim Det As Double
        ' AA = distance(Noeuds(leselements(el).Noeud(8)).coord(1), Noeuds(leselements(el).Noeud(8)).coord(2), Noeuds(leselements(el).Noeud(1)).coord(1), Noeuds(leselements(el).Noeud(1)).coord(2))
        '  BB = distance(Noeuds(leselements(el).Noeud(2)).coord(1), Noeuds(leselements(el).Noeud(2)).coord(2), Noeuds(leselements(el).Noeud(1)).coord(1), Noeuds(leselements(el).Noeud(1)).coord(2))
        Dim E As Double = Material.E0 '* (1 - MoyenneTenseur(DamageParameter))
        Call FormD3(Material.V, E)

        For iu = 1 To 24
            For ju = 1 To 24
                Ke(iu, ju) = 0
            Next
        Next

        'matrice de rigidité 
        For ie As Integer = 1 To Ngp
            For je As Integer = 1 To Ngp
                For le As Integer = 1 To Ngp

                    Call Fmlin3(le, je, ie, Samp)

                    'jacobien matrix
                    For ii = 1 To 3
                        For ji = 1 To 3
                            X = 0
                            For f = 1 To 8
                                X += DER(ii, f) * ListNoeud(f - 1).Coord(ji)
                            Next
                            JAC(ii, ji) = X
                        Next
                    Next
                    'Déterminant de la matrice jacobéenne

                    Det = JAC(1, 1) * DetM(JAC(2, 2), JAC(2, 3), JAC(3, 2), JAC(3, 3)) -
                          JAC(1, 2) * DetM(JAC(2, 1), JAC(2, 3), JAC(3, 1), JAC(3, 3)) +
                          JAC(1, 3) * DetM(JAC(2, 1), JAC(2, 2), JAC(3, 1), JAC(3, 2))

                    ' Det = AA * BB
                    'matrice inverse de la matrice jacobéenne
                    JAC1(1, 1) = JAC(2, 2) * JAC(3, 3) - JAC(3, 2) * JAC(2, 3)
                    JAC1(2, 1) = -JAC(2, 1) * JAC(3, 3) + JAC(3, 1) * JAC(2, 3)
                    JAC1(3, 1) = JAC(2, 1) * JAC(3, 2) - JAC(3, 1) * JAC(2, 2)
                    JAC1(1, 2) = -JAC(1, 2) * JAC(3, 3) + JAC(3, 2) * JAC(1, 3)
                    JAC1(2, 2) = JAC(1, 1) * JAC(3, 3) - JAC(3, 1) * JAC(1, 3)
                    JAC1(3, 2) = -JAC(1, 1) * JAC(3, 2) + JAC(3, 1) * JAC(1, 2)
                    JAC1(1, 3) = JAC(1, 2) * JAC(2, 3) - JAC(2, 2) * JAC(1, 3)
                    JAC1(2, 3) = -JAC(1, 1) * JAC(2, 3) + JAC(2, 1) * JAC(1, 3)
                    JAC1(3, 3) = JAC(1, 1) * JAC(2, 2) - JAC(2, 1) * JAC(1, 2)

                    For f = 1 To 3
                        For l = 1 To 3
                            JAC1(f, l) = JAC1(f, l) / Det
                        Next
                    Next

                    '--------------rest a verifié

                    For ii = 1 To 3
                        For ji = 1 To 8
                            X = 0
                            For f = 1 To 3
                                X += JAC1(ii, f) * DER(f, ji)
                            Next
                            DERIV(ii, ji) = X
                        Next
                    Next

                    For ij = 1 To 24
                        For ii = 1 To 6
                            BEE(ii, ij) = 0
                        Next
                    Next
                    Call FormB3()

                    For ii = 1 To 6
                        For JJ = 1 To 24
                            X = 0
                            For kk = 1 To 6
                                X += DEE(ii, kk) * BEE(kk, JJ)
                            Next
                            DBEE(ii, JJ) = X
                        Next JJ
                    Next ii


                    For il = 1 To 6
                        For jl = 1 To 24
                            BT(jl, il) = BEE(il, jl)
                        Next jl
                    Next il
                    For im = 1 To 24
                        For jm = 1 To 24
                            X = 0
                            For ki = 1 To 6
                                X += BT(im, ki) * DBEE(ki, jm)
                            Next ki
                            BTDB(im, jm) = X

                        Next
                    Next



                    QUOT = Det * Samp(ie, 2) * Samp(je, 2) * Samp(le, 2)

                    For id = 1 To 24
                        For jd = 1 To 24
                            BTDB(id, jd) = BTDB(id, jd) * QUOT
                        Next
                    Next
                    For ic = 1 To 24
                        For jc = 1 To 24
                            Ke(ic, jc) = Ke(ic, jc) + BTDB(ic, jc)
                        Next
                    Next

                Next
            Next
        Next



        Return Ke
    End Function
    Sub FormB3()

        Dim KI, M, L, N As Integer
        Dim X, Y, Z As Double
        For M = 1 To 8
            N = 3 * M
            KI = N - 1
            L = KI - 1

            X = DERIV(1, M)
            BEE(1, L) = X
            BEE(4, KI) = X
            BEE(6, N) = X

            Y = DERIV(2, M)
            BEE(2, KI) = Y
            BEE(4, L) = Y
            BEE(5, N) = Y

            Z = DERIV(3, M)
            BEE(3, N) = Z
            BEE(5, KI) = Z
            BEE(6, L) = Z
        Next
        Exit Sub
    End Sub
    Private Sub Fmlin3(i As Integer, j As Integer, k As Integer, Samp(,) As Double)

        Dim eta, xi, Zeta, etam, etap, xim, Zetam, Zetap, xip As Double

        xi = Samp(i, 1)
        eta = Samp(j, 1)
        Zeta = Samp(k, 1)

        etam = 1 - eta
        xim = 1 - xi
        Zetam = 1 - Zeta

        etap = eta + 1
        xip = xi + 1
        Zetap = Zeta + 1

        FUN(1) = 0.125 * etam * xim * Zetam
        FUN(2) = 0.125 * etam * xip * Zetam
        FUN(3) = 0.125 * etap * xip * Zetam
        FUN(4) = 0.125 * etap * xim * Zetam
        FUN(5) = 0.125 * etam * xim * Zetap
        FUN(6) = 0.125 * etam * xip * Zetap
        FUN(7) = 0.125 * etap * xip * Zetap
        FUN(8) = 0.125 * etap * xim * Zetap

        DER(1, 1) = -0.125 * etam * Zetam
        DER(1, 4) = -0.125 * etap * Zetam
        DER(1, 3) = 0.125 * etap * Zetam
        DER(1, 2) = 0.125 * etam * Zetam
        DER(1, 5) = -0.125 * etam * Zetap
        DER(1, 8) = -0.125 * etap * Zetap
        DER(1, 7) = 0.125 * etap * Zetap
        DER(1, 6) = 0.125 * etam * Zetap

        DER(2, 1) = -0.125 * xim * Zetam
        DER(2, 4) = 0.125 * xim * Zetam
        DER(2, 3) = 0.125 * xip * Zetam
        DER(2, 2) = -0.125 * xip * Zetam
        DER(2, 5) = -0.125 * xim * Zetap
        DER(2, 8) = 0.125 * xim * Zetap
        DER(2, 7) = 0.125 * xip * Zetap
        DER(2, 6) = -0.125 * xip * Zetap

        DER(3, 1) = -0.125 * xim * etam
        DER(3, 4) = -0.125 * xim * etap
        DER(3, 3) = -0.125 * xip * etap
        DER(3, 2) = -0.125 * xip * etam
        DER(3, 5) = 0.125 * xim * etam
        DER(3, 8) = 0.125 * xim * etap
        DER(3, 7) = 0.125 * xip * etap
        DER(3, 6) = 0.125 * xip * etam

    End Sub

    Private Sub FmQuad(i As Integer, j As Integer, Samp(,) As Double)

        Dim eta, xi, etam, etap, xim, xip As Double

        eta = Samp(i, 1)
        xi = Samp(j, 1)

        etam = 0.25 * (1 - eta)
        xim = 0.25 * (1 - xi)
        etap = 0.25 * (eta + 1)
        xip = 0.25 * (xi + 1)

        FUNQuad(1) = 4 * xim * etam
        FUNQuad(4) = 4 * xim * etap
        FUNQuad(3) = 4 * xip * etap
        FUNQuad(2) = 4 * xip * etam

        DerQuad(1, 1) = -etam
        DerQuad(1, 4) = -etap
        DerQuad(1, 3) = etap
        DerQuad(1, 2) = etam
        DerQuad(2, 1) = -xim
        DerQuad(2, 4) = xim
        DerQuad(2, 3) = xip
        DerQuad(2, 2) = -xip

    End Sub
    Private Sub FormD3(v As Double, E As Double)
        Dim vv, v2 As Double

        For i = 1 To 6
            For j = 1 To 6
                DEE(i, j) = 0
            Next
        Next

        Dim v1 As Double

        v1 = v / (1 - v)
        vv = (1 - 2 * v) / (2 * (1 - v))

        DEE(1, 1) = 1
        DEE(2, 2) = 1
        DEE(3, 3) = 1
        DEE(1, 2) = v1
        DEE(2, 1) = v1
        DEE(1, 3) = v1
        DEE(3, 1) = v1
        DEE(2, 3) = v1
        DEE(3, 2) = v1
        DEE(4, 4) = vv
        DEE(5, 5) = vv
        DEE(6, 6) = vv
        For iv = 1 To 6
            For jv = 1 To 6
                DEE(iv, jv) = DEE(iv, jv) * E / (2.0 * (1.0 + v) * vv)
            Next
        Next


    End Sub
    Public Function DerivativeFaillerFunction(LoiComportement As Integer, Stress() As Double, a As Double, b As Double, Theta As Double,
                                         I1 As Double, J2 As Double, J3 As Double) As Double()
        Dim Erate(0) As Double
        Select Case LoiComportement
            Case 0 'Concrete Damaged Plasticity
                Erate = Material.DerivativeYieldFunctionStress(Stress, a, b, Theta, I1, J2, J3)
        End Select

        Return Erate

    End Function
    Public Function PlasticStrain(LoiComportement As Integer, PsiDegre As Double, Excent As Double, SigmaT0 As Double, Strain() As Double,
                             Stress() As Double, Delastic(,) As Double, StrainIncrement() As Double, InElasticStrain As Double, DInElasticStrain As Double, SigmaTbarre As Double, SigmaMax As Double,
                                  fact As Double) As Double()
        Select Case LoiComportement
            Case 0 'Concrete Damaged Plasticity
                'Return Material.PlasticStress(Strain, Stress, Delastic, StrainIncrement, InElasticStrain, DInElasticStrain, SigmaTbarre, SigmaMax, fact)
        End Select
        Return Nothing
    End Function
    Public Function DerivativePotentialFunction(LoiComportement As Integer, Stress() As Double, PsiDegre As Double, Excent As Double,
                                                SigmaT0 As Double, Optional YieldFunction As Double = 1, Optional Type As Integer = 1) As Double()
        Dim Erate(0) As Double
        Select Case LoiComportement
            Case 0 'Concrete Damaged Plasticity
                'damaged paramates estimation
                Erate = Material.DerivativePotentialFunction(Stress, PsiDegre, Excent, SigmaT0, Type)
        End Select
        If YieldFunction <> 1 Then
            For i = 1 To 6
                Erate(i) = Erate(i) * YieldFunction
            Next
        End If
        Return Erate
    End Function
End Class

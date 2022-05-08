Imports System.Math
Public Class PDM
    Inherits Base
    Public E0, V, Fb0_Fc0, Kc, PsiDegre, Leq, Fck, Excent, SigmaT0, SigmaC, SigmaT, Dc, Dt, Damaged As Double
    Public ac, bc, at, bt As Double

    Public Function ConcreteDamagedPlasticityYieldCondition(stress() As Double, SigmaI As Double, SigmaII As Double, SigmaIII As Double, Fb0_Fc0 As Double,
                                                            SigmaC As Double, SigmaT As Double, Kc As Double) As Double

        Dim J2, I1 As Double
        InvariantsContraintes(stress, I1,,, J2)
        Dim Alfa, Beta, Gamma As Double
        ParamatersCDP(Fb0_Fc0, SigmaC, SigmaT, Kc, Alfa, Beta, Gamma)
        Dim SigmaMax As Double
        SigmaMax = Max(Max(SigmaI, SigmaII), SigmaIII)
        If SigmaMax >= 0 Then
            Dim result As Double = (1 / (1 - Alfa))
            result = result * (Sqrt(3 * J2) + Alfa * I1 + Beta * SigmaMax) - SigmaC
            Return result
        Else
            Dim result As Double = (1 / (1 - Alfa))
            result = result * (Sqrt(3 * J2) + Alfa * I1 - Gamma * SigmaMax)
            result = result - SigmaC
            Return result
        End If
    End Function

    Public Function DLambda(DStrain() As Double, DerivativeQ() As Double, DerivativeF() As Double,
                            Delastic(,) As Double, InElasticStrain As Double, SigmaTbarre As Double,
                            SigmaMax As Double, DInElasticStrain As Double) As Double
        'Explicit case
        Dim DDQ() As Double = MatriceVecteurMultipe(Delastic, DerivativeQ, 1)
        Dim DDE() As Double = MatriceVecteurMultipe(Delastic, DStrain, 1)
        Dim fc0 As Double = 0.4 * (Fck + 8)
        Dim DerivativeFStrain As Double = DerivativeYieldFunctionStrain(InElasticStrain, fc0, SigmaTbarre, SigmaMax)
        Dim FirstScale As Double = 0
        Dim SecondScale As Double = 0
        Dim IH As Integer = DerivativeQ.Count - 1
        For i = 1 To IH
            FirstScale += DerivativeF(i) * DDE(i)
        Next
        FirstScale += DerivativeFStrain * DInElasticStrain
        For i = 1 To IH
            SecondScale += DerivativeF(i) * DDQ(i)
        Next
        ' SecondScale += DerivativeFStrain * InElasticStrain
        Dim DLam As Double = FirstScale / SecondScale
        ' Dim DLam As Double = F / SecondScale
        If DLam < 0 Then DLam = 0
        Return DLam
    End Function
    Public Function DLambda(Approch As Integer, Strain() As Double, Stress() As Double, DSigmaE() As Double, Delastic(,) As Double,
                            Comprission As List(Of DoublePoint), Tension As List(Of DoublePoint),
                            InElasticStrain As Double, SigmaTbarre As Double, SigmaMax As Double,
                            ByRef DerivativeQ() As Double) As Double
        'Explicit case
        Dim a, b, Gamma, I1, J2, Theta, J3, SigmaI, SigmaII, SigmaIII As Double

        ValeurPropre(Stress, SigmaI, SigmaII, SigmaIII, Theta)
        Dim DefPlasT, DefPlasC As Double

        DamageParametres(Approch, Strain, SigmaI, SigmaII, SigmaIII, Fck, Leq, bc, bt, SigmaC, SigmaT, SigmaT0, Dc, Dt, Damaged, Comprission, Tension, E0, DefPlasT, DefPlasC)
        SigmaC = SigmaC / (1 - Dc)
        SigmaT = SigmaT / (1 - Dt)
        ParamatersCDP(Fb0_Fc0, SigmaC, SigmaT, Kc, a, b, Gamma)
        InvariantsContraintes(Stress, I1,,, J2, J3)
        If SigmaI < 0 Then b = -Gamma
        SigmaT0 = 0.3016 * Fck ^ 0.6666667
        Dim DerivativeF() As Double = DerivativeYieldFunctionStress(Stress, a, b, Theta, I1, J2, J3)
        DerivativeQ = DerivativePotentialFunction(Stress, PsiDegre, Excent, SigmaT0)
        Dim DFSigma As Double = VecteurVecteurMultipe(DerivativeF, DSigmaE)
        Dim DDQ() As Double = MatriceVecteurMultipe(Delastic, DerivativeQ)
        Dim DFDDQ As Double = VecteurVecteurMultipe(DerivativeF, DDQ)
        Dim DLam As Double = DFSigma / DFDDQ
        If DLam < 0 Then DLam = 0
        Return DLam
    End Function
    Public Function DLambda(NewF As Double, Strain() As Double, DerivativeQ() As Double, DerivativeF() As Double,
                             Delastic(,) As Double, InElasticStrain As Double, DInElasticStrain As Double, SigmaTbarre As Double,
                            SigmaMax As Double) As Double
        'Implicit case
        Dim DDQ() As Double = MatriceVecteurMultipe(Delastic, DerivativeQ)
        '  Dim fc0 As Double = 0.4 * (Fck + 8)
        '  Dim DerivativeFStrain As Double = DerivativeYieldFunctionStrain(InElasticStrain, fc0, SigmaTbarre, SigmaMax)
        '  Dim A As Double = DerivativeFStrain * InElasticStrain
        Dim FirstScale As Double = VecteurVecteurMultipe(DerivativeF, DDQ)
        Dim DLam As Double = NewF / FirstScale
        If DLam < 0 Then DLam = 0
        Return DLam
    End Function
    Public Function PlasticStressImplicit(Strain() As Double, Stress() As Double, Delastic(,) As Double,
                                  StrainIncrement() As Double, InElasticStrain As Double, DInElasticStrain As Double, SigmaTbarre As Double,
                                  SigmaMax As Double, alfa As Double) As Double()
        Dim a, b, Gamma, I1, J2, Theta, J3, SigmaI, SigmaII, SigmaIII As Double

        Dim Comprission As List(Of DoublePoint)
        Dim Tension As List(Of DoublePoint)
        SigmaT0 = 0.3016 * Fck ^ 0.6666667
        ValeurPropre(Stress, SigmaI, SigmaII, SigmaIII, Theta)
        InvariantsContraintes(Stress, I1,,, J2, J3)
        Dim DefPlasT, DefPlasC As Double
        DamageParametres(1, Strain, SigmaI, SigmaII, SigmaIII, Fck, Leq, bc, bt, SigmaC, SigmaT, SigmaT0, Dc, Dt, Damaged, Comprission, Tension, E0, DefPlasT, DefPlasC)
        SigmaC = SigmaC / (1 - Dc)
        SigmaT = SigmaT / (1 - Dt)

        ParamatersCDP(Fb0_Fc0, SigmaC, SigmaT, Kc, a, b, Gamma)

        If SigmaI < 0 Then b = -Gamma
        Dim DerivativeF() As Double = DerivativeYieldFunctionStress(Stress, a, b, Theta, I1, J2, J3)
        Dim DerivativeQ() As Double = DerivativePotentialFunction(Stress, PsiDegre, Excent, SigmaT0)
        Dim Lamda As Double = DLambda(StrainIncrement, DerivativeQ, DerivativeF, Delastic, InElasticStrain, SigmaTbarre, SigmaMax, DInElasticStrain)
        Dim IH As Integer = DerivativeQ.Count - 1
        For i = 1 To IH
            DerivativeQ(i) = DerivativeQ(i) * Lamda '/ alfa
        Next

        Return MatriceVecteurMultipe(Delastic, DerivativeQ)
    End Function
    Public Function PlasticStress(Approch As Integer, NewF As Double, Strain() As Double, Stress() As Double, Delastic(,) As Double,
                                  StrainIncrement() As Double, InElasticStrain As Double, DInElasticStrain As Double, SigmaTbarre As Double,
                                  Comprission As List(Of DoublePoint), Tension As List(Of DoublePoint)) As Double()
        ' For implicit Stress = Stress0 + DStress
        Dim a, b, Gamma, I1, J2, Theta, J3, SigmaI, SigmaII, SigmaIII As Double
        ValeurPropre(Stress, SigmaI, SigmaII, SigmaIII, Theta)
        InvariantsContraintes(Stress, I1,,, J2, J3)
        Dim DefPlasT, DefPlasC As Double
        DamageParametres(Approch, Strain, SigmaI, SigmaII, SigmaIII, Fck, Leq, bc, bt, SigmaC, SigmaT, SigmaT0, Dc, Dt, Damaged, Comprission, Tension, E0, DefPlasT, DefPlasC)
        SigmaC = SigmaC / (1 - Dc)
        SigmaT = SigmaT / (1 - Dt)
        ParamatersCDP(Fb0_Fc0, SigmaC, SigmaT, Kc, a, b, Gamma)

        Dim SigmaMax As Double = Max(Max(SigmaI, SigmaII), SigmaIII)
        If SigmaMax < 0 Then b = -Gamma
        Dim DerivativeF() As Double = DerivativeYieldFunctionStress(Stress, a, b, Theta, I1, J2, J3)
        Dim DerivativeQ() As Double = DerivativePotentialFunction(Stress, PsiDegre, Excent, SigmaT0)

        Dim Lamda As Double = DLambda(NewF, Strain, DerivativeQ, DerivativeF, Delastic, InElasticStrain, DInElasticStrain, SigmaTbarre, SigmaMax)
        DerivativeQ = ScaleVectorMultipe(Lamda, DerivativeQ)
        Return MatriceVecteurMultipe(Delastic, DerivativeQ)
    End Function

    Public Function CorrectPlasticStress(Tol As Double, Approch As Integer, Strain() As Double, Stress() As Double, Delastic(,) As Double,
                                  StrainIncrement() As Double, InElasticStrain As Double, DInElasticStrain As Double, SigmaTbarre As Double,
                                  Comprission As List(Of DoublePoint), Tension As List(Of DoublePoint), SIGMA() As Double,
                                         OldStress() As Double, Optional ReturnCompleteStress As Boolean = False) As Double()


        Dim a, b, Gamma, I1, J2, Theta, J3, SigmaI, SigmaII, SigmaIII As Double
        Dim OldF As Double
        Dim NewF As Double
        Dim NewStress() As Double
        NewStress = CopyVector(Stress)
        For i = 1 To 10
            ValeurPropre(NewStress, SigmaI, SigmaII, SigmaIII, Theta)
            OldF = YieldFunctionEstimation(Approch, Strain, NewStress, SigmaI, SigmaII, SigmaIII, Fck, Leq, bc, bt, Fb0_Fc0, Kc, Comprission, Tension, SigmaC, SigmaT, SigmaT0, Dc, Dt, Damaged, E0, InElasticStrain)
            Dim PlasticStressTensor() As Double = PlasticStress(Approch, OldF, Strain, NewStress, Delastic, StrainIncrement, InElasticStrain, DInElasticStrain, SigmaTbarre, Comprission, Tension)
            NewStress = VecAdd(NewStress, PlasticStressTensor, -1)
            ValeurPropre(NewStress, SigmaI, SigmaII, SigmaIII, Theta)
            NewF = YieldFunctionEstimation(Approch, Strain, NewStress, SigmaI, SigmaII, SigmaIII, Fck, Leq, bc, bt, Fb0_Fc0, Kc, Comprission, Tension, SigmaC, SigmaT, SigmaT0, Dc, Dt, Damaged, E0, InElasticStrain)
            If NewF > OldF Then
                NewStress = VecAdd(NewStress, PlasticStressTensor, 1)

                ValeurPropre(NewStress, SigmaI, SigmaII, SigmaIII, Theta)
                InvariantsContraintes(NewStress, I1,,, J2, J3)
                Dim DefPlasT, DefPlasC As Double
                DamageParametres(Approch, Strain, SigmaI, SigmaII, SigmaIII, Fck, Leq, bc, bt, SigmaC, SigmaT, SigmaT0, Dc, Dt, Damaged, Comprission, Tension, E0, DefPlasT, DefPlasC)
                SigmaC = SigmaC / (1 - Dc)
                SigmaT = SigmaT / (1 - Dt)
                ParamatersCDP(Fb0_Fc0, SigmaC, SigmaT, Kc, a, b, Gamma)
                If SigmaI < 0 Then b = -Gamma

                Dim DerivativeF() As Double = DerivativeYieldFunctionStress(NewStress, a, b, Theta, I1, J2, J3)
                Dim FirstItem As Double = VecteurVecteurMultipe(DerivativeF, DerivativeF)
                Dim Dlamda As Double = OldF / FirstItem
                PlasticStressTensor = ScaleVectorMultipe(Dlamda, DerivativeF)
                NewStress = VecAdd(NewStress, PlasticStressTensor, -1)
                ValeurPropre(NewStress, SigmaI, SigmaII, SigmaIII, Theta)
                NewF = YieldFunctionEstimation(Approch, Strain, NewStress, SigmaI, SigmaII, SigmaIII, Fck, Leq, bc, bt, Fb0_Fc0, Kc, Comprission, Tension, SigmaC, SigmaT, SigmaT0, Dc, Dt, Damaged, E0, InElasticStrain)

            End If

            If NewF < Tol Then
                If ReturnCompleteStress Then
                    Return NewStress
                Else
                    Return VecAdd(VecAdd(SIGMA, OldStress), NewStress, -1)
                End If

                Exit For
            End If
        Next
        If ReturnCompleteStress Then
            Return NewStress
        Else
            Return VecAdd(VecAdd(SIGMA, OldStress), NewStress, -1)
        End If
    End Function
    Public Function YieldFunctionEstimation(Approch As Integer, Strain() As Double, Stress() As Double,
                                           SigmaI As Double, SigmaII As Double, SigmaIII As Double, Fck As Double, Ieq As Double, bcBakhti As Double, btBakhti As Double, Fb0_Fc0 As Double,
                                           Kc As Double, Comprission As List(Of DoublePoint), Tension As List(Of DoublePoint),
                                            ByRef SigmaC As Double, ByRef SigmaT As Double, ByRef SigmaT0 As Double,
                                            ByRef Dc As Double, ByRef Dt As Double, ByRef D As Double,
                                            Optional Ebeton As Double = 0, ByRef Optional DefPlasC As Double = 0) As Double
        Dim YieldFunction As Double = 0

        'Damaged Paramaters Evaluation
        Dim DefPlasT As Double
        Dim Resultats As Boolean = DamageParametres(Approch, Strain, SigmaI, SigmaII, SigmaIII, Fck, Ieq, bcBakhti, btBakhti,
                                                            SigmaC, SigmaT, SigmaT0, Dc, Dt, D, Comprission, Tension, Ebeton, DefPlasT, DefPlasC)
        If Dc = 1 Then Dc = 0.999999999
        If Dt = 1 Then Dt = 0.999999999
        SigmaC = SigmaC / (1 - Dc)
        SigmaT = SigmaT / (1 - Dt)
        YieldFunction = ConcreteDamagedPlasticityYieldCondition(Stress, SigmaI, SigmaII, SigmaIII, Fb0_Fc0, SigmaC, SigmaT, Kc)
        Return YieldFunction
    End Function
    Public Function DerivativeYieldFunctionStrain(InElasticStrain As Double, fc0 As Double, SigmaTBare As Double, SigmaMax As Double) As Double
        Dim Resultats As Double = (Crocher(SigmaMax) / SigmaTBare) - 1
        Resultats = Resultats * fc0 * ac * (ac + 1) * (ac + 2) * bc * Exp(bc * InElasticStrain)
        Dim S As Double = ((2 * ac + 2) * Exp(bc * InElasticStrain) - ac) ^ 2
        Return Resultats / S
    End Function
    Public Function DerivativeYieldFunctionStress(Stress() As Double, a As Double, b As Double, Theta As Double, I1 As Double, J2 As Double, J3 As Double) As Double()
        Dim Resultats(6) As Double
        Dim M1(0, 0), M2(0, 0), M3(6, 6), Flow(6, 6), x As Double
        FormM3D(Stress, M1, M2, M3)
        Dim J As Double = Sqrt(J2)
        Dim p As Double = I1 / 3
        Dim FirstSide() As Double = DerativeMeanStress()
        Dim SecondSide() As Double = DerativeDiviatoricStress(Stress, p, J)
        Dim ThirdSide() As Double = DerativeTheta(Stress, p, J, Theta, J3, M3)
        Dim DQ1, DQ2, DQ3 As Double
        DQ1 = (3 * a) / (1 - a) ' (3 * a + b) / (1 - a)
        Dim Sqrt3 As Double = Sqrt(3)
        DQ2 = 2 * b * Sin(Theta - 2 * PI / 3)
        DQ2 = Sqrt3 + DQ2 / Sqrt3
        DQ2 = Sqrt3 / (1 - a) 'DQ2 / (1 - a)
        DQ3 = 2 * b * J * Cos(Theta - 2 * PI / 3)
        DQ3 = 0 ' DQ3 / (Sqrt3 * (1 - a))
        For i = 1 To 6
            Resultats(i) = DQ1 * FirstSide(i) + DQ2 * SecondSide(i) + DQ3 * ThirdSide(i)
        Next
        Return Resultats
    End Function
    Public Function DplasticConcreteDamagedPlacticity(Approch As Integer, Delastic(,) As Double, PsiDegre As Double, Excent As Double, Strain() As Double,
                            Stress() As Double, Fck As Double, Ieq As Double, bcBakhti As Double, btBakhti As Double, Fb0_Fc0 As Double, Kc As Double, Comprission As List(Of DoublePoint),
        Tension As List(Of DoublePoint), Ebeton As Double) As Double(,)
        Dim D(0, 0) As Double
        Dim a, b, Gamma, I1, J2, Theta, J3, SigmaT0, Dc, Dt, Damaged, SigmaC, SigmaT, SigmaI, SigmaII, SigmaIII As Double
        ValeurPropre(Stress, SigmaI, SigmaII, SigmaIII, Theta)
        Dim DefPlasT, DefPlasC As Double

        DamageParametres(Approch, Strain, SigmaI, SigmaII, SigmaIII, Fck, Ieq, bcBakhti, btBakhti, SigmaC, SigmaT,
                       SigmaT0, Dc, Dt, Damaged, Comprission, Tension, Ebeton, DefPlasT, DefPlasC)
        SigmaC = SigmaC / (1 - Dc)
        SigmaT = SigmaT / (1 - Dt)
        ParamatersCDP(Fb0_Fc0, SigmaC, SigmaT, Kc, a, b, Gamma)
        InvariantsContraintes(Stress, I1,,, J2, J3)
        If SigmaI < 0 Then b = -Gamma


        Dim DerivativeQ() As Double = DerivativePotentialFunction(Stress, PsiDegre, Excent, SigmaT0)
        Dim DerivativeF() As Double = DerivativeYieldFunctionStress(Stress, a, b, Theta, I1, J2, J3)
        Dim Fc0 As Double = 0.4 * (Fck + 8)
        Dim Ft0 As Double = 0.3016 * Fck ^ (2 / 3)
        Dim SigmaMax As Double = Max(Max(SigmaI, SigmaII), SigmaIII)
        Dim DerivativeFToStrain As Double = DerivativeYieldFunctionStrain(DefPlasC, Fc0, SigmaT, SigmaMax)

        Dim Ddqds() As Double = MatriceVecteurMultipe(Delastic, DerivativeQ, 1)
        Dim Dfdsd() As Double = MatriceVecteurMultipe(Delastic, DerivativeF, 1)
        Dim Denom As Double = VecteurVecteurMultipe(Dfdsd, DerivativeQ, 1)
        Dim Aparameter As Double = -DerivativeFToStrain * DerivativeQ(1)  ' = VecteurVecteurMultipe(DerivativeFToStrain, DerivativeF, 1)
        Dim IH As Integer = Ddqds.Count - 1
        ReDim D(IH, IH)
        For i = 1 To IH
            For J = 1 To IH
                D(i, J) = Ddqds(i) * Dfdsd(J) / (Denom + Aparameter)
            Next
        Next
        Return D
    End Function

    Public Function ModifiedEulerSchemeWithSubstepping(Approch As Integer, OldStress() As Double,
                                                       SIGMA() As Double, Alfa As Double, EPS() As Double,
                                                       OldStrain() As Double, Delastic(,) As Double,
                                                       Comprission As List(Of DoublePoint), Tension As List(Of DoublePoint),
                                                       InElasticStrain As Double, SigmaMax As Double, SigmaTbarre As Double,
                                                            SSTOL As Double, YTOL As Double) As Double()

        Dim DSigmeE() As Double = ScaleVectorMultipe(1 - Alfa, SIGMA)
        Dim DStrainE() As Double = ScaleVectorMultipe(1 - Alfa, EPS)
        Dim DSigmeE0() As Double = ScaleVectorMultipe(Alfa, SIGMA)
        Dim DStrainE0() As Double = ScaleVectorMultipe(Alfa, EPS)

        Dim DT, Time, DLamda As Double
        Dim DSigme1(), DSigme2(), Strain(), Stress(), DStressTemp(), PlasticStress() As Double
        Dim DG(), R, q, NewF, SigmaI, SigmaII, SigmaIII, SigmaC, SigmaT, SigmaT0, Dc, DTen, D As Double
        DT = 1
        Time = 0
        Strain = VecAdd(OldStrain, DStrainE0)
        Stress = VecAdd(OldStress, DSigmeE0)
        Do While Time < 1
            DLamda = DT * DLambda(Approch, Strain, Stress, DSigmeE, Delastic, Comprission, Tension, InElasticStrain, SigmaTbarre, SigmaMax, DG)
            DSigme1 = VecAdd(ScaleVectorMultipe(DT, DSigmeE), MatriceVecteurMultipe(ScaleMatrixMultipe(DLamda, Delastic), DG), -1)
            DLamda = DT * DLambda(Approch, Strain, VecAdd(Stress, DSigme1), DSigmeE, Delastic, Comprission, Tension, InElasticStrain, SigmaTbarre, SigmaMax, DG)
            DSigme2 = VecAdd(ScaleVectorMultipe(DT, DSigmeE), MatriceVecteurMultipe(ScaleMatrixMultipe(DLamda, Delastic), DG), -1)
            DStressTemp = VecAdd(Stress, ScaleVectorMultipe(0.5, VecAdd(DSigme1, DSigme2), 1))
            R = Max((NormeVecteur(VecAdd(DSigme2, DSigme1, -1)) / 2 * NormeVecteur(DStressTemp)), 0.00001)

            If R <= SSTOL Then
                Stress = CopyVector(DStressTemp)
                'Add code to check if F<0
                ValeurPropre(Stress, SigmaI, SigmaII, SigmaIII)
                SigmaMax = Max(Max(SigmaI, SigmaII), SigmaIII)
                NewF = YieldFunctionEstimation(Approch, Strain, Stress, SigmaI, SigmaII, SigmaIII, Fck, Leq, bc, bt, Fb0_Fc0, Kc, Comprission, Tension, SigmaC, SigmaT, SigmaT0, Dc, DTen, D, E0, InElasticStrain)
                If NewF > 0.01 Then
                    Stress = CorrectPlasticStress(0.01, Approch, Strain, Stress, Delastic, EPS, InElasticStrain, InElasticStrain, SigmaT, Comprission, Tension, SIGMA, OldStress, True)
                End If
                q = Min(Min(0.9 * Sqrt(SSTOL / R), 1.1), 1)
                DT = q * DT
                DT = Min(DT, 1 - Time)
                Time += DT
                Exit Do
            Else
                q = Max(Min(0.9 * Sqrt(SSTOL / R), 2), 0.1)
                DT = q * DT
            End If
        Loop
        Return Stress
    End Function
    Public Function DerivativePotentialFunction(Stress() As Double, PsiDegre As Double, Epsilon As Double, Sigmat0 As Double,
                                                   Optional Type As Integer = 0) As Double()
        Select Case Type
            Case 0

                Dim TanPsi As Double = Tan(DegreToRadian(PsiDegre))
                Dim Resultats(6) As Double
                Dim M1(0, 0), M2(0, 0), M3(6, 6), Flow(6, 6), x As Double
                Dim DQ1, DQ2 As Double
                Dim J2 As Double
                InvariantsContraintes(Stress,,,, J2)
                DQ1 = TanPsi
                DQ2 = 3 / (2 * Sqrt((Epsilon * Sigmat0 * TanPsi) ^ 2 + 3 * J2))
                FormM3D(Stress, M1, M2, M3)
                For i = 1 To 6
                    For j = 1 To 6
                        Flow(i, j) = (M1(i, j) * DQ1 + M2(i, j) * DQ2)
                    Next
                Next
                For i = 1 To 6
                    x = 0
                    For j = 1 To 6
                        x += Flow(i, j) * Stress(j)
                    Next
                    Resultats(i) = x
                Next
                Return Resultats
            Case 1

                Dim TanPsi As Double = Tan(DegreToRadian(PsiDegre))
                Dim Resultats(6), I1, J2, J3 As Double
                InvariantsContraintes(Stress, I1,,, J2, J3)
                Dim J As Double = Sqrt(J2)
                Dim p As Double = I1 / 3
                Dim FirstSide() As Double = DerativeMeanStress()
                Dim SecondSide() As Double = DerativeDiviatoricStress(Stress, p, J)

                Dim DQ1, DQ2 As Double
                DQ1 = TanPsi
                DQ2 = 3 * J / Sqrt(3 * J ^ 2 + (Epsilon * Sigmat0 * TanPsi) ^ 2)

                For i = 1 To 6
                    Resultats(i) = DQ1 * FirstSide(i) + DQ2 * SecondSide(i)
                Next
                Return Resultats
        End Select
        Return Nothing
    End Function
    Public Function DamageParametres(Approch As Integer, Strain() As Double, SigmaI As Double, SigmaII As Double,
                                SigmaIII As Double, Fck As Double, Ieq As Double, bcBakhti As Double, btBakhti As Double, ByRef SigmaC As Double,
                                ByRef SigmaT As Double, ByRef SigmaT0 As Double, ByRef Dc As Double, ByRef Dt As Double, ByRef D As Double,
                                 Comprission As List(Of DoublePoint), Tension As List(Of DoublePoint), Ebeton As Double, ByRef DefPlasT As Double,
                                     ByRef DefPlasC As Double) As Boolean
        'Selon B.Approch & al. 2017, New methodology for calculating damage variables evolution in plastic damage model for RC structures ,     Hc = 0.9 and Ht = 0
        Select Case Approch
            Case 0  'Alfarah approach

                DamageParametres = True
                Dim r As Double = Rfunction(SigmaI, SigmaII, SigmaIII)
                Dim St, Sc As Double 'Must evaluate  Dc,Dt  'Eq 19 & 20
                Dim Hc, Ht As Double
                Dim StrI, StrII, StrIII As Double

                SigmaT = 0
                SigmaT0 = 0
                SigmaC = 0
                Dc = 0
                Dt = 0
                D = 0



                ValeurPropre(Strain, StrI, StrII, StrIII)

                Dim DefT As Double = r * Max(Max(StrI, StrII), StrIII)
                Dim DefC As Double = -(1 - r) * Min(Min(StrI, StrII), StrIII)
                Dim DefCpl, DefTpl As Double



                Hc = 0.9
                Ht = 0
                Dim b As Double = 0.9
                Dim Newb As Double
                Dim Fcm, Ftm, Fc0, Ft0, Defcm, Deftm, Eci, E0, Gf, Gch, Wc As Double
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
                bc = (1.97 * Fcm / Gch) * Ieq
                bt = (0.453 * Fck ^ (2 / 3) / Gf) * Ieq

                Sc = 1 - Hc * (1 - r)
                St = 1 - Ht * r



                Dim ElaticDefor As Double = 0.4 * Fcm / E0
                Dim iter As Integer = 0
                Do 'Lancer iteration
                    iter += 1
                    If DefC >= 0 And DefC <= ElaticDefor Then
                        SigmaC = SigmaCI(DefC, E0)
                        DefPlasC = 0
                        '   If DefC <> 0 Then DamageParametres = False
                    ElseIf DefC > ElaticDefor And DefC <= Defcm Then
                        SigmaC = SigmaCII(DefC, Defcm, Fcm, Eci)
                        DefPlasC = DefC - SigmaC / E0
                    ElseIf DefC > Defcm Then
                        SigmaC = SigmaCIII(DefC, Defcm, Fcm, Gch, Ieq, b, E0)
                        DefPlasC = DefC - SigmaC / E0
                    End If
                    If DefT >= 0 And DefT <= Deftm Then
                        SigmaT = SigmaTI(DefT, E0)
                        DefPlasT = 0
                        '   If DefT <> 0 Then DamageParametres = False
                    ElseIf DefT > Deftm Then
                        SigmaT = SigmaTII(DefT, Deftm, Ieq, Wc, Ftm)
                        DefPlasT = DefT - SigmaT / E0
                    End If


                    Dc = 1 - (1 / (2 + ac)) * (2 * (1 + ac) * Exp(-bc * DefPlasC) - ac * Exp(-2 * bc * DefPlasC))
                    Dt = 1 - (1 / (2 + at)) * (2 * (1 + at) * Exp(-bt * DefPlasT) - at * Exp(-2 * bt * DefPlasT))
                    D = 1 - (1 - St * Dc) * (1 - Sc * Dt)
                    If Dc < 0 Then Dc = 0
                    If Dt < 0 Then Dt = 0
                    If D < 0 Then D = 0
                    If DefPlasC = 0 Then
                        SigmaC = Fc0
                        DefCpl = 0
                    Else
                        DefCpl = DefPlasC - SigmaC * Dc / (E0 * (1 - Dc))
                        If DefCpl < 0 Then DefCpl = 0
                    End If

                    If DefPlasT = 0 Then
                        SigmaT = Ft0
                        DefTpl = 0
                    Else
                        DefTpl = DefPlasT - SigmaT * Dt / (E0 * (1 - Dt))
                    End If


                    If DefPlasC <> 0 Then
                        Newb = DefCpl / DefPlasC
                        If Convergence(Newb, b, 0.0001) Or iter > 400 Then
                            Exit Do
                        End If
                        b = Newb
                    Else
                        Exit Do
                    End If
                Loop

                SigmaT0 = SigmaTI(Deftm, E0)




            Case 1 'Bakhti approach
                DamageParametres = True
                Dim r As Double = Rfunction(SigmaI, SigmaII, SigmaIII)
                If SigmaI > 0 Then
                    SigmaI += 0
                End If
                Dim St, Sc As Double 'Must evaluate  Dc,Dt  'Eq 19 & 20
                Dim Hc, Ht As Double
                Dim StrI, StrII, StrIII As Double
                SigmaT = 0
                SigmaT0 = 0
                SigmaC = 0
                Dc = 0
                Dt = 0
                D = 0

                ValeurPropre(Strain, StrI, StrII, StrIII)
                Dim DefT As Double = r * Max(Max(StrI, StrII), StrIII)
                Dim DefC As Double = -(1 - r) * Min(Min(StrI, StrII), StrIII)
                Dim SigT As Double = r * Max(Max(SigmaI, SigmaII), SigmaIII)
                Dim SigC As Double = -(1 - r) * Min(Min(SigmaI, SigmaII), SigmaIII)

                Hc = 0.9
                Ht = 0
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
                bc = bcBakhti
                bt = btBakhti

                Sc = 1 - Hc * (1 - r)
                St = 1 - Ht * r

                DefPlasC = FindInelasticStrain(0, 0.05, 0.00001, Fc0, ac, bc, E0, DefC)
                DefPlasT = FindInelasticStrain(0, 0.005, 0.000001, Ft0, at, bt, E0, DefT)

                Dc = 1 - (2 * (1 + ac) * Exp(-bc * DefPlasC) - ac * Exp(-2 * bc * DefPlasC)) / (2 + ac)
                Dt = 1 - (2 * (1 + at) * Exp(-bt * DefPlasT) - at * Exp(-2 * bt * DefPlasT)) / (2 + at)
                D = 1 - (1 - St * Dc) * (1 - Sc * Dt)
                If DefPlasC = 0 Then
                    SigmaC = Fc0
                Else
                    SigmaC = Fc0 * ((1 + ac) * Exp(-bc * DefPlasC) - ac * Exp(-2 * bc * DefPlasC))
                End If
                If DefPlasT = 0 Then
                    SigmaT = Ft0
                Else
                    SigmaT = Ft0 * ((1 + at) * Exp(-bt * DefPlasT) - at * Exp(-2 * bt * DefPlasT))
                End If

                SigmaT0 = SigmaTI(Deftm, E0)
                If Dc < 0 Then Dc = 0
                If Dt < 0 Then Dt = 0
                If D < 0 Then D = 0
            Case 2 'From curves
                DamageParametres = True
                Dim r As Double = Rfunction(SigmaI, SigmaII, SigmaIII)

                Dim St, Sc As Double 'Must evaluate  Dc,Dt  'Eq 19 & 20
                Dim Hc, Ht As Double
                Dim StrI, StrII, StrIII As Double

                SigmaT = 0
                SigmaT0 = 0
                SigmaC = 0
                Dc = 0
                Dt = 0
                D = 0


                ValeurPropre(Strain, StrI, StrII, StrIII)
                Dim DefT As Double = r * Max(Max(StrI, StrII), StrIII)
                Dim DefC As Double = -(1 - r) * Min(Min(StrI, StrII), StrIII)

                Hc = 0.9
                Ht = 0

                Sc = 1 - Hc * (1 - r)
                St = 1 - Ht * r

                DefPlasC = FindInelasticStrainFromCurve(DefC, Comprission, Ebeton)
                DefPlasT = FindInelasticStrainFromCurve(DefT, Tension, Ebeton)

                SigmaC = FindStressFromCurve(DefPlasC, Comprission, Dc)
                SigmaT = FindStressFromCurve(DefPlasT, Tension, Dt, SigmaT0)

                D = 1 - (1 - St * Dc) * (1 - Sc * Dt)
                If Dc < 0 Then Dc = 0
                If Dt < 0 Then Dt = 0
                If D < 0 Then D = 0
        End Select
    End Function
    Public Function FindInelasticStrain(IntrevalStart As Double, IntrevalEnd As Double, Precesion As Double,
                                        Fc0 As Double, ac As Double, bc As Double, E0 As Double,
                                        TotalStrain As Double) As Double

        Dim IntervalMidde As Double = (IntrevalStart + IntrevalEnd) / 2
        'Find total strain intervale
        Dim TotalIntrevalStart, TotalIntrevalEnd, TotalMiddelInterval As Double
        Dim SigmaC1, SigmaC2, SigmaC3 As Double
        SigmaC1 = Fc0 * ((1 + ac) * Exp(-bc * IntrevalStart) - ac * Exp(-2 * bc * IntrevalStart))
        SigmaC2 = Fc0 * ((1 + ac) * Exp(-bc * IntervalMidde) - ac * Exp(-2 * bc * IntervalMidde))
        SigmaC3 = Fc0 * ((1 + ac) * Exp(-bc * IntrevalEnd) - ac * Exp(-2 * bc * IntrevalEnd))
        TotalIntrevalStart = IntrevalStart + SigmaC1 / E0
        TotalMiddelInterval = IntervalMidde + SigmaC2 / E0
        TotalIntrevalEnd = IntrevalEnd + SigmaC3 / E0
        If TotalStrain < TotalIntrevalStart Then
            Return 0
        End If

        If Abs(TotalStrain - TotalIntrevalStart) < Precesion Then
            Return IntrevalStart
        End If
        If Abs(TotalStrain - TotalMiddelInterval) < Precesion Then
            Return IntervalMidde
        End If
        If Abs(TotalStrain - TotalIntrevalEnd) < Precesion Then
            Return IntrevalEnd
        End If

        If TotalStrain > TotalIntrevalStart And TotalStrain < TotalMiddelInterval Then
            Return FindInelasticStrain(IntrevalStart, IntervalMidde, Precesion, Fc0, ac, bc, E0, TotalStrain)
        End If
        If TotalStrain > TotalMiddelInterval And TotalStrain < TotalIntrevalEnd Then
            Return FindInelasticStrain(IntervalMidde, IntrevalEnd, Precesion, Fc0, ac, bc, E0, TotalStrain)
        End If
        Return Nothing
    End Function
    Public Function FindInelasticStrainFromCurve(Strain As Double, Curve As List(Of DoublePoint), E0 As Double) As Double
        Dim Str1, Str2 As Double
        Dim Sigma, Str As Double
        For i = 0 To Curve.Count - 2
            Str1 = Curve.Item(i).x + Curve.Item(i).y / E0
            Str2 = Curve.Item(i + 1).x + Curve.Item(i + 1).y / E0
            If Strain >= Str1 And Strain <= Str2 Then
                Sigma = Curve.Item(i).y + (Strain - Str1) * (Curve.Item(i + 1).y - Curve.Item(i).y) / (Str2 - Str1)
                Str = Strain - Sigma / E0
                If Str > 0 Then
                    Return Str
                Else
                    Return 0
                End If
            End If
        Next

        If Strain >= Curve.Item(Curve.Count - 1).x + Curve.Item(Curve.Count - 1).y / E0 Then
            Return Curve.Item(Curve.Count - 1).x
        End If

        Return 0
    End Function

End Class

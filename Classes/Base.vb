Imports MathNet.Numerics.LinearAlgebra
Imports MathNet.Numerics.LinearAlgebra.Double
Imports MathNet.Numerics.LinearAlgebra.Complex
Imports System.Math
Public Class Base
    Structure DoublePoint
        Public x As Double
        Public y As Double
    End Structure

    Public Function DerativeMeanStress() As Double()
        Dim Resultat(6) As Double
        Resultat(1) = 1 / 3
        Resultat(2) = 1 / 3
        Resultat(3) = 1 / 3
        Return Resultat
    End Function
    Public Function DerativeDiviatoricStress(Stress() As Double, P As Double, J As Double) As Double()
        Dim Resultat(6) As Double
        Resultat(1) = (Stress(1) - P) / (2 * J)
        Resultat(2) = (Stress(2) - P) / (2 * J)
        Resultat(3) = (Stress(3) - P) / (2 * J)
        Resultat(4) = Stress(4) / J
        Resultat(5) = Stress(5) / J
        Resultat(6) = Stress(6) / J
        Return Resultat
    End Function

    Public Function DerativeTheta(Stress() As Double, P As Double, J As Double, Theta As Double, J3 As Double, M3(,) As Double) As Double()
        Dim Resultat(6) As Double
        Dim Cons As Double = Sqrt(3) / (2 * Cos(3 * Theta) * J ^ 3)
        Dim FirstSide() As Double = DerativeDiviatoricStress(Stress, P, J)
        For i = 1 To FirstSide.Count - 1
            FirstSide(i) = FirstSide(i) * J3 / J
        Next
        Dim SecondSide(6) As Double
        For i = 1 To 6
            Dim x As Double
            For J = 1 To 6
                x += M3(i, J) * Stress(J)
            Next
            SecondSide(i) = x
        Next
        For i = 1 To 6
            Resultat(i) = FirstSide(i) - SecondSide(i)
            Resultat(i) = Resultat(i) * Cons
        Next
        Return Resultat
    End Function


    Public Function CopyVector(V() As Double) As Double()
        Return V
    End Function
    Public Sub FormM3D(Stress() As Double, ByRef M1(,) As Double, ByRef M2(,) As Double, ByRef M3(,) As Double)
        Dim SX As Double = Stress(1)
        Dim SY As Double = Stress(2)
        Dim SZ As Double = Stress(3)
        Dim TXY As Double = Stress(4)
        Dim TYZ As Double = Stress(5)
        Dim TZX As Double = Stress(6)
        Dim Sigm As Double = (SX + SY + SZ) / 3
        Dim DX As Double = SX - Sigm
        Dim DY As Double = SY - Sigm
        Dim DZ As Double = SZ - Sigm
        ReDim M1(6, 6)
        ReDim M2(6, 6)
        ReDim M3(6, 6)

        For i = 1 To 3
            M2(i, i) = 2
            M2(i + 3, i + 3) = 6
            For j = 1 To 3
                M1(i, j) = 1 / (3 * Sigm)
            Next
        Next
        M2(1, 2) = -1
        M2(1, 3) = -1
        M2(2, 3) = -1

        M2(2, 1) = -1
        M2(3, 1) = -1
        M2(3, 2) = -1

        'M3
        M3(1, 1) = DX
        M3(1, 2) = DZ
        M3(1, 3) = DY
        M3(1, 4) = TXY
        M3(1, 5) = -2 * TYZ
        M3(1, 6) = TZX

        M3(2, 1) = DZ
        M3(2, 2) = DY
        M3(2, 3) = DX
        M3(2, 4) = TXY
        M3(2, 5) = TYZ
        M3(2, 6) = -2 * TZX

        M3(3, 1) = DY
        M3(3, 2) = DX
        M3(3, 3) = DZ
        M3(3, 4) = -2 * TXY
        M3(3, 5) = TYZ
        M3(3, 6) = TZX

        M3(4, 1) = TXY
        M3(4, 2) = TXY
        M3(4, 3) = -2 * TXY
        M3(4, 4) = -3 * DZ
        M3(4, 5) = 3 * TZX
        M3(4, 6) = 3 * TYZ

        M3(5, 1) = -2 * TYZ
        M3(5, 2) = TYZ
        M3(5, 3) = TYZ
        M3(5, 4) = 3 * TZX
        M3(5, 5) = -3 * DX
        M3(5, 6) = 3 * TXY

        M3(6, 1) = TZX
        M3(6, 2) = -2 * TZX
        M3(6, 3) = TZX
        M3(6, 4) = 3 * TYZ
        M3(6, 5) = 3 * TXY
        M3(6, 6) = -3 * DY



        For i = 1 To 6
            For j = 1 To 6
                M1(i, j) = M1(i, j) / 3
                M2(i, j) = M2(i, j) / 3
                M3(i, j) = M3(i, j) / 3
            Next
        Next

        ' For i = 1 To 6
        'MsgBox(M2(i, 1) & "   " & M2(i, 2) & "   " & M2(i, 3) & "   " & M2(i, 4) & "   " & M2(i, 5) & "   " & M2(i, 6))
        '  Next

    End Sub
    Public Function DSegmaM(Stress() As Double, DSigm As Integer) As Double
        ' DSigm = 1 ==> DSegmaM/DSigmX , DSigm = 2 ==> DSegmaM/DSigmY ,DSigm = 3 ==> DSegmaM/DSigmZ ,DSigm = 4 ==> DSegmaM/DTouYZ ,DSigm = 5 ==> DSegmaM/DTouXZ ,DSigm = 6 ==> DSegmaM/DTouXY
        Select Case DSigm
            Case 1
                Return 1 / 3
                Exit Function
            Case 2
                Return 1 / 3
                Exit Function
            Case 3
                Return 1 / 3
                Exit Function
            Case 4
                Return 0
                Exit Function
            Case 5
                Return 0
                Exit Function
            Case 6
                Return 0
                Exit Function
        End Select
        Return 0
    End Function
    Public Function DSegmaBare(Stress() As Double, DSigm As Integer) As Double
        ' DSigm = 1 ==> DSegmaBare/DSigmX , DSigm = 2 ==> DSegmaBare/DSigmY ,DSigm = 3 ==> DSegmaBare/DSigmZ ,DSigm = 4 ==> DSegmaBare/DTouYZ ,DSigm = 5 ==> DSegmaBare/DTouXZ ,DSigm = 6 ==> DSegmaBare/DTouXY
        Dim Df As Double = Sqrt(0.5 * ((Stress(1) - Stress(2)) ^ 2 + (Stress(2) - Stress(3)) ^ 2 + (Stress(3) - Stress(1)) ^ 2 + 6 * Stress(4) ^ 2 + 6 * Stress(5) ^ 2 + 6 * Stress(6) ^ 2))
        Dim Fprime As Double
        Select Case DSigm
            Case 1
                Fprime = 0.5 * (2 * (Stress(1) - Stress(2)) - 2 * (Stress(3) - Stress(1)))
            Case 2
                Fprime = 0.5 * (2 * (Stress(2) - Stress(3)) - 2 * (Stress(1) - Stress(2)))
            Case 3
                Fprime = 0.5 * (2 * (Stress(3) - Stress(1)) - 2 * (Stress(2) - Stress(3)))
            Case 4
                Fprime = 12 * Stress(4)
            Case 5
                Fprime = 12 * Stress(5)
            Case 6
                Fprime = 12 * Stress(6)
        End Select
        Return Fprime / Df
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
            If Xproj <= Math.Max(Xd1, Xd2) And Xproj >= Min(Xd1, Xd2) Then Cond1 = True
        Else
            If Math.Abs(Xproj - Xd1) < 0.001 Then Cond1 = True
        End If
        If Yd1 <> Yd2 Then
            If Yproj <= Math.Max(Yd1, Yd2) And Yproj >= Min(Yd1, Yd2) Then Cond2 = True
        Else
            If Math.Abs(Yproj - Yd1) < 0.001 Then Cond2 = True
        End If
        If Cond1 And Cond2 Then
            AppartientSegmentDroite = True
        End If
    End Function
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
    Public Function Distance(x1 As Double, y1 As Double, x2 As Double, y2 As Double) As Double
        Distance = ((x2 - x1) ^ 2 + (y2 - y1) ^ 2) ^ 0.5
    End Function

    Public Function Distance3D(x1 As Double, y1 As Double, z1 As Double, x2 As Double, y2 As Double, z2 As Double) As Double
        Distance3D = ((x2 - x1) ^ 2 + (y2 - y1) ^ 2 + (z2 - z1) ^ 2) ^ 0.5
    End Function

    Public Shared Sub EquationDroiteApartir2Points(Xd1 As Double, Yd1 As Double, Xd2 As Double, Yd2 As Double, ByRef A As Double, ByRef B As Double, ByRef C As Double)
        ' A X +B Y + C = 0
        If Xd1 = Xd2 And Yd1 = Yd2 Then
            B = 0
            A = 0
            C = 0
            Exit Sub
        End If

        If Xd1 = Xd2 Then
            If Yd1 <> Yd2 Then
                B = 0
                A = 1
                C = -Xd1
                Exit Sub
            End If
        End If

        If Yd1 = Yd2 Then
            If Xd1 <> Xd2 Then
                B = 1
                A = 0
                C = -Yd1
                Exit Sub
            End If
        End If


        A = (Yd2 - Yd1) / (Xd2 - Xd1)
        B = -1
        C = (-B * Yd1 - A * Xd1)
    End Sub
    Public Function Rfunction(V1 As Double, V2 As Double, V3 As Double) As Double
        Return (Crocher(V1) + Crocher(V2) + Crocher(V3)) / (Abs(V1) + Abs(V2) + Abs(V3))
    End Function


    Public Function MohrCoulomb(Phi As Double, Theta As Double, SigmaBare As Double, Sigm As Double,
                                 C As Double) As Double

        'This subroutine calculates the value of the yield function
        'for a Mohr-Coulomb material (phi in degrees).


        Dim phir As Double = Phi * PI / 180
        Dim snph As Double = Sin(phir)
        Dim csph As Double = Cos(phir)
        Dim csth As Double = Cos(Theta)
        Dim snth As Double = Sin(Theta)
        Return snph * Sigm + SigmaBare * (csth / Sqrt(3) - snth * snph / 3) - C * csph

    End Function




    Public Function FindStressFromCurve(InelasticStrain As Double, Curve As List(Of DoublePoint),
                                        ByRef d As Double, ByRef Optional SigmaT0 As Double = 0) As Double
        Dim Pick As DoublePoint = PickPoint(Curve)
        Dim Sigma As Double
        For i = 0 To Curve.Count - 2
            If InelasticStrain >= Curve.Item(i).x And InelasticStrain <= Curve.Item(i + 1).x Then
                Sigma = Curve.Item(i).y + (InelasticStrain - Curve.Item(i).x) * (Curve.Item(i + 1).y - Curve.Item(i).y) / (Curve.Item(i + 1).x - Curve.Item(i).x)
                Exit For
            End If
        Next

        If InelasticStrain >= Curve.Item(Curve.Count - 1).x Then
            Sigma = Curve.Item(Curve.Count - 1).y
        End If

        SigmaT0 = Pick.y
        If InelasticStrain <= Pick.x Then
            d = 0
        Else
            d = 1 - Sigma / Pick.y
        End If
        Return Sigma
    End Function
    Private Function PickPoint(Curve As List(Of DoublePoint)) As DoublePoint
        Dim Id As Integer = -1
        Dim ZMAX As Double = -999999999
        For i = 0 To Curve.Count - 1
            If Curve.Item(i).y > ZMAX Then
                Id = i
                ZMAX = Curve.Item(i).y
            End If
        Next
        If Id <> -1 Then
            Return Curve.Item(Id)
        Else
            Return Nothing
        End If
    End Function



    Public Function SigmaT0Estimation(Fck As Double) As Double
        Return 0.3016 * Fck ^ (2 / 3)
    End Function
    Public Function SigmaTII(StreT As Double, StreTm As Double, Ieq As Double, Wc As Double, Ftm As Double) As Double
        Dim W As Double = (StreT - StreTm) * Ieq
        Dim result As Double = 1 + (3 * W / Wc) ^ 3
        result = result * Exp(-6.93 * W / Wc)
        result = result - (28 * W / Wc) * Exp(-6.93)
        Return Ftm * result
    End Function
    Public Function SigmaTI(StreT As Double, E0 As Double) As Double
        Return StreT * E0
    End Function
    Public Function SigmaCIII(StreC As Double, StreCm As Double, Fcm As Double, Gch As Double, Ieq As Double, b As Double, E0 As Double) As Double
        Dim GamaC As Double = (Fcm * StreCm * PI ^ 2) / (2 * ((Gch / Ieq) - 0.5 * Fcm * (StreCm * (1 - b) + b * (Fcm / E0))) ^ 2)

        Return (((2 + GamaC * Fcm * StreCm) / (2 * Fcm)) - GamaC * StreC + (GamaC * StreC ^ 2) / (2 * StreCm)) ^ -1
    End Function
    Public Function SigmaCII(StreC As Double, StreCm As Double, Fcm As Double, Eci As Double) As Double
        Return ((Eci * StreC / Fcm) - (StreC / StreCm) ^ 2) / (1 + ((Eci * StreCm / Fcm) - 2) * StreC / StreCm) * Fcm
    End Function
    Public Function SigmaCI(StreC As Double, E0 As Double) As Double
        Return StreC * E0
    End Function


    Public Function RendreString(val As Double, Pre As Integer) As String
        RendreString = ""
        Select Case Pre
            Case 0
                RendreString = Format(val, "#").ToString
            Case 1
                RendreString = Format(val, "#.0").ToString
            Case 2
                RendreString = Format(val, "#.00").ToString
            Case 3
                RendreString = Format(val, "#.000").ToString
            Case 4
                RendreString = Format(val, "#.0000").ToString
            Case 5
                RendreString = Format(val, "#.00000").ToString
        End Select
        If Math.Abs(val) < 1 Then RendreString = "0" & RendreString
    End Function
    Public Function DplasticMohrCoulomb(phi As Double, psi As Double, dee(,) As Double, stress() As Double) As Double(,)


        'This subroutine forms the plastic stress/strain matrix
        'for a Mohr-Coulomb material (phi, psi in degrees).

        Dim dfds(6), dqds(6), ddqds(), dfdsd() As Double
        Dim t1, t2, t3, t4, t5, t6, t8, t10, t12, t13, t14, t15, t16, t18, t19, t20, t21, t22, t23, t24, t25, t26, t27, t29,
            t32, t33, t35, t37, t38, t39, t40, t41, t42, t43, t45, t48, t50, t51, t53, t56, t60, t61, t63, t64, t69, t70, t73,
            t77, t79, t80, t82, t83, t86, t92, t93, t94, t98, t103, t106, t110, t113, t122, t133, t145, t166, t186, t206, pm,
            phir, snph, snth, sq3, sx, sy, sz, txy, tyz, tzx, psir, snps As Double




        phir = phi * PI / 180
        snph = Sin(phir)
        psir = psi * PI / 180
        snps = Sin(psir)
        sq3 = Sqrt(3)

        sx = stress(1)
        sy = stress(2)
        sz = stress(3)
        txy = stress(4)
        tyz = stress(5)
        tzx = stress(6)
        t3 = 1 / 3
        t4 = (sx + sy + sz) * t3
        t5 = sx - t4
        t6 = sy - t4
        t8 = sz - t4
        t10 = tyz ^ 2
        t12 = tzx ^ 2
        t14 = txy ^ 2
        t23 = (sx - sy) ^ 2
        t25 = (sy - sz) ^ 2
        t27 = (sz - sx) ^ 2
        t32 = Sqrt((t23 + t25 + t27) / 6 + t14 + t10 + t12)
        t33 = t32 ^ 2
        t37 = 3 * sq3 * (t5 * t6 * t8 - t5 * t10 - t6 * t12 - t8 * t14 + 2 * txy * tyz * tzx) / 2 / t33 / t32
        If t37 > 1 Then t37 = 1
        If t37 < -1 Then t37 = -1
        t38 = Asin(t37)
        t40 = Sin(t38 * t3)
        snth = -t40
        If (Abs(snth) > 0.49) Then
            pm = -1
            If (snth < 0) Then pm = 1
            t2 = snph / 3
            t4 = (sx - sy) ^ 2
            t6 = (sy - sz) ^ 2
            t8 = (sz - sx) ^ 2
            t10 = 1 / 6
            t12 = txy ^ 2
            t13 = tyz ^ 2
            t14 = tzx ^ 2
            t16 = Sqrt((t4 + t6 + t8) * t10 + t12 + t13 + t14)
            t19 = 1 / t16 / sq3
            t20 = 1 / 2
            t21 = t19 * t20
            t23 = 3 + pm * snph
            t25 = 2 * sy
            t26 = 2 * sz
            t33 = 2 * sx
            t48 = t20 * t23
            dfds(1) = t2 + t21 * t23 * (4 * sx - t25 - t26) * t10 / 2
            dfds(2) = t2 + t21 * t23 * (-t33 + 4 * sy - t26) * t10 / 2
            dfds(3) = t2 + t21 * t23 * (-t25 + 4 * sz - t33) * t10 / 2
            dfds(4) = t19 * t48 * txy
            dfds(5) = t19 * t48 * tyz
            dfds(6) = t19 * t48 * tzx
            t2 = snps / 3
            t23 = 3 + pm * snps
            t48 = t20 * t23
            dqds(1) = t2 + t21 * t23 * (4 * sx - t25 - t26) * t10 / 2
            dqds(2) = t2 + t21 * t23 * (-t33 + 4 * sy - t26) * t10 / 2
            dqds(3) = t2 + t21 * t23 * (-t25 + 4 * sz - t33) * t10 / 2
            dqds(4) = t19 * t48 * txy
            dqds(5) = t19 * t48 * tyz
            dqds(6) = t19 * t48 * tzx
        Else
            t1 = 1 / 3
            t2 = snph * t1
            t4 = (sx - sy) ^ 2
            t6 = (sy - sz) ^ 2
            t8 = (sz - sx) ^ 2
            t10 = 1 / 6
            t12 = txy ^ 2
            t13 = tyz ^ 2
            t14 = tzx ^ 2
            t15 = (t4 + t6 + t8) * t10 + t12 + t13 + t14
            t16 = Sqrt(t15)
            t18 = 3 * sq3
            t20 = (sx + sy + sz) * t1
            t21 = sx - t20
            t22 = sy - t20
            t23 = t21 * t22
            t24 = sz - t20
            t29 = 2 * txy
            t32 = t23 * t24 - t21 * t13 - t22 * t14 - t24 * t12 + t29 * tyz * tzx
            t33 = 1 / 2
            t35 = t16 ^ 2
            t37 = 1 / t35 / t16
            t39 = t18 * t32 * t33 * t37
            If (t39 > 1) Then t39 = 1
            If (t39 < -1) Then t39 = -1
            t40 = Asin(t39)
            t41 = t40 * t1
            t42 = Cos(t41)
            t43 = Sin(t41)
            t45 = 1 / sq3
            t48 = 1 / t16 * (t42 + t43 * snph * t45)
            t50 = 2 * sy
            t51 = 2 * sz
            t53 = (4 * sx - t50 - t51) * t10
            t56 = 1 - t1
            t60 = t21 * t1 * t24
            t61 = t23 * t1
            t63 = t1 * t14
            t64 = t1 * t12
            t69 = t18 * t32
            t70 = t35 ^ 2
            t73 = t33 / t70 / t16
            t77 = t18 * (t56 * t22 * t24 - t60 - t61 - t56 * t13 + t63 + t64) * t33 * t37 - 3 / 2 * t69 * t73 * t53
            t79 = 3 ^ 2
            t80 = sq3 ^ 2
            t82 = t32 ^ 2
            t83 = 2 ^ 2
            t86 = t15 ^ 2
            t92 = Sqrt(1 - t79 * t80 * t82 / t83 / t86 / t15)
            t93 = 1 / t92
            t94 = t93 * t1
            t98 = t2 * t45
            t103 = 2 * sx
            t106 = (-t103 + 4 * sy - t51) * t10
            t110 = t1 * t22 * t24
            t113 = t1 * t13
            t122 = t18 * (-t110 + t21 * t56 * t24 - t61 + t113 - t56 * t14 + t64) * t33 * t37 - 3 / 2 * t69 * t73 * t106
            t133 = (-t50 + 4 * sz - t103) * t10
            t145 = t18 * (-t110 - t60 + t23 * t56 + t113 + t63 - t56 * t12) * t33 * t37 - 3 / 2 * t69 * t73 * t133
            t166 = t18 * (-2 * t24 * txy + 2 * tyz * tzx) * t33 * t37 - 3 * t69 * t73 * txy
            t186 = t18 * (-2 * t21 * tyz + t29 * tzx) * t33 * t37 - 3 * t69 * t73 * tyz
            t206 = t18 * (-2 * t22 * tzx + t29 * tyz) * t33 * t37 - 3 * t69 * t73 * tzx
            dfds(1) = t2 + t48 * t53 / 2 + t16 * (-t43 * t77 * t94 + t42 * t77 * t93 * t98)
            dfds(2) = t2 + t48 * t106 / 2 + t16 * (-t43 * t122 * t94 + t42 * t122 * t93 * t98)
            dfds(3) = t2 + t48 * t133 / 2 + t16 * (-t43 * t145 * t94 + t42 * t145 * t93 * t98)
            dfds(4) = t48 * txy + t16 * (-t43 * t166 * t94 + t42 * t166 * t93 * t98)
            dfds(5) = t48 * tyz + t16 * (-t43 * t186 * t94 + t42 * t186 * t93 * t98)
            dfds(6) = t48 * tzx + t16 * (-t43 * t206 * t94 + t42 * t206 * t93 * t98)
            t2 = snps * t1
            t48 = 1 / t16 * (t42 + t43 * snps * t45)
            t98 = t2 * t45
            dqds(1) = t2 + t48 * t53 / 1 + t16 * (-t43 * t77 * t94 + t42 * t77 * t93 * t98)
            dqds(2) = t2 + t48 * t106 / 1 + t16 * (-t43 * t122 * t94 + t42 * t122 * t93 * t98)
            dqds(3) = t2 + t48 * t133 / 1 + t16 * (-t43 * t145 * t94 + t42 * t145 * t93 * t98)
            dqds(4) = t48 * txy + t16 * (-t43 * t166 * t94 + t42 * t166 * t93 * t98)
            dqds(5) = t48 * tyz + t16 * (-t43 * t186 * t94 + t42 * t186 * t93 * t98)
            dqds(6) = t48 * tzx + t16 * (-t43 * t206 * t94 + t42 * t206 * t93 * t98)
        End If
        ddqds = MatriceVecteurMultipe(dee, dqds, 1)
        dfdsd = MatriceVecteurMultipe(dee, dfds, 1)
        Dim denom As Double = VecteurVecteurMultipe(dfdsd, dqds)
        Dim PL(6, 6) As Double
        For i = 1 To 6
            For j = 1 To 6
                PL(i, j) = ddqds(i) * dfdsd(j) / denom
            Next
        Next
        Return PL

    End Function




    Public Shared Function ModuleElasticite(Fck As Double) As Double
        Dim Fcm As Double = Fck + 8
        Dim Eci As Double = 10000 * Fcm ^ (1 / 3)
        'Resultat en Mpa
        Return Eci * (0.8 + 0.2 * (Fcm / 88))
    End Function
    Public Function NormeVecteur(Vect() As Double) As Double
        If IsNothing(Vect) Or Vect.Count = 0 Then Return 0
        Dim X As Double
        For i = 0 To Vect.Count - 1
            X += Vect(i) ^ 2
        Next
        Return Sqrt(X)
    End Function

    Public Function StepTime(LoiComportement As Integer, Approch As Integer, Alfa As Single, Delastic(,) As Double, PsiDegre As Double, Excent As Double, Strain() As Double,
                              Stress() As Double, Fck As Double, Ieq As Double, bcBakhti As Double, btBakhti As Double, Fb0_Fc0 As Double, Kc As Double,
                             Comprission As List(Of DoublePoint), Tension As List(Of DoublePoint), Ebeton As Double) As Double
        Dim D(0, 0) As Double
        Dim a, b, Gamma, I1, J2, Theta, J3, SigmaT0, Dc, Dt, Damaged, SigmaC, SigmaT, SigmaI, SigmaII, SigmaIII As Double
        ValeurPropre(Stress, SigmaI, SigmaII, SigmaIII, Theta)
        Dim DefPlasT, DefPlasC As Double
        '  DamageParametres(Approch, Strain, SigmaI, SigmaII, SigmaIII, Fck, Ieq, bcBakhti, btBakhti, SigmaC, SigmaT, SigmaT0, Dc, Dt, Damaged, Comprission, Tension, Ebeton, DefPlasT, DefPlasC)
        'SigmaC = SigmaC / (1 - Dc)
        'SigmaT = SigmaT / (1 - Dt)

        'ParamatersCDP(Fb0_Fc0, SigmaC, SigmaT, Kc, a, b, Gamma)
        'InvariantsContraintes(Stress, I1,,, J2, J3)
        If SigmaI < 0 Then
            b = -Gamma
        End If
        'Dim DerivativeF() As Double = DerivativeFaillerFunction(LoiComportement, Stress, a, b, Theta, I1, J2, J3)
        'Dim DerivativeQ() As Double = DerivativePotentialFunction(LoiComportement, Stress, PsiDegre, Excent, SigmaT0)
        'Dim Ddqds() As Double = MatriceVecteurMultipe(Delastic, DerivativeQ, 1)
        ' Return Alfa / VecteurVecteurMultipe(DerivativeF, Ddqds)
        MsgBox("goto StepTime subroutine to complete your work")
    End Function

    Public Function StepTime(DruckerPracher As Boolean, PsiDegre As Double, v As Double, E As Double, Lmin As Double, Rou As Double) As Double
        If DruckerPracher Then
            Dim SinPsi As Double = Sin(DegreToRadian(PsiDegre))
            Dim a As Double = (1 + v) * (1 - 2 * v) * (3 - SinPsi) ^ 2
            Dim b As Double = E * (0.75 * (1 - 2 * v) * (3 - SinPsi) ^ 2 + 6 * (1 + v) * SinPsi ^ 2)
            Return a / b
        Else
            Dim Lamda As Double = v * E / ((1 + v) * (1 - 2 * v))
            Dim Mu As Double = E / (2 * (1 + v))
            Return Lmin / (((Lamda + 2 * Mu) / 1) ^ 0.5)
        End If

    End Function
    Public Sub PotentialFunctionColomb(PsiDegre As Double, ThetaRadian As Double, DSBAR As Double,
                                       ByRef DQ1 As Double, ByRef DQ2 As Double, ByRef DQ3 As Double)

        Dim SinPsi As Double = Sin(DegreToRadian(PsiDegre))
        Dim SinTheta As Double = Sin(ThetaRadian)
        Dim SQ3 As Double = Sqrt(3)
        Dim C1 As Integer
        DQ1 = SinPsi
        If Abs(SinTheta) > 0.49 Then
            C1 = 1
            If SinTheta < 0 Then C1 = -1
            DQ2 = (SQ3 * 0.5 - C1 * SinPsi * 0.5 / SQ3) * SQ3 * 0.5 / DSBAR
            DQ3 = 0
        Else
            Dim CosTheta As Double = Cos(ThetaRadian)
            Dim Cos3Theta As Double = Cos(3 * ThetaRadian)
            Dim Tan3Theta As Double = Tan(3 * ThetaRadian)
            Dim TanTheta As Double = SinTheta / CosTheta
            DQ2 = SQ3 * CosTheta / DSBAR * ((1 + TanTheta * Tan3Theta) + SinPsi * (Tan3Theta - TanTheta) / SQ3) * 0.5
            DQ3 = 1.5 * (SQ3 * SinTheta + SinPsi * CosTheta) / (Cos3Theta * DSBAR * DSBAR)
        End If
    End Sub

    Public Function MoyenneTenseur(Tenseur() As Double) As Double
        Dim Resultats As Double
        For i = 1 To Tenseur.Count - 1
            Resultats += Tenseur(i)
        Next
        Return Resultats / (Tenseur.Count - 1)
    End Function

    Public Sub ParamatersCDP(Fb0_Fc0 As Double, SigmaC As Double, SigmaT As Double, Kc As Double, ByRef Alfa As Double,
                              ByRef Beta As Double, ByRef Gamma As Double)
        Alfa = ((Fb0_Fc0) - 1) / (2 * (Fb0_Fc0) - 1)
        Beta = (SigmaC / SigmaT) * (1 - Alfa) - (1 + Alfa)
        Gamma = 3 * (1 - Kc) / (2 * Kc - 1)
    End Sub
    Public Function MCrocher(X As Double) As Double
        Return 0.5 * (X - Abs(X))
    End Function
    Public Function Crocher(X As Double) As Double
        Return 0.5 * (X + Abs(X))
    End Function
    Public Function DegreToRadian(Angle) As Double
        Return PI * Angle / 180
    End Function
    Public Sub InvariantsContraintes(Stress() As Double,
                                    Optional ByRef I1 As Double = 0, Optional ByRef I2 As Double = 0,
                                    Optional ByRef I3 As Double = 0, Optional ByRef J2 As Double = 0,
                                    Optional ByRef J3 As Double = 0)
        '----------I1
        I1 = Stress(1) + Stress(2) + Stress(3)
        '----------I2
        I2 = Stress(1) * Stress(2) + Stress(3) * Stress(2) + Stress(1) * Stress(3)
        I2 = I2 - Stress(4) ^ 2 - Stress(5) ^ 2 - Stress(6) ^ 2
        '----------I3
        I3 = Stress(1) * Stress(2) * Stress(3)
        I3 += 2 * Stress(4) * Stress(5) * Stress(6)
        I3 -= Stress(3) * Stress(4) ^ 2   '6
        I3 -= Stress(1) * Stress(5) ^ 2   '4
        I3 -= Stress(2) * Stress(6) ^ 2   '5
        '----------J2 AND J3
        J2 = (I1 ^ 2 / 3) - I2
        J3 = (2 * I1 ^ 2 / 27) - (I1 * I2 / 3) + I3

    End Sub
    Public Sub ValeurPropre(Tensor() As Double, Optional ByRef V1 As Double = 0, Optional ByRef V2 As Double = 0,
                                      Optional ByRef V3 As Double = 0, Optional ByRef THETA As Double = 0, Optional ByRef SigmaBare As Double = 0,
                                      Optional ByRef SigmaM As Double = 0, Optional ByRef I1 As Double = 0, Optional ByRef I2 As Double = 0,
                                      Optional ByRef I3 As Double = 0, Optional ByRef J2 As Double = 0, Optional ByRef J3 As Double = 0)


        Dim Sqrt3 As Double = Sqrt(3)
        SigmaM = (Tensor(1) + Tensor(2) + Tensor(3)) / 3
        Dim d2 As Double = ((Tensor(1) - Tensor(2)) ^ 2 + (Tensor(2) - Tensor(3)) ^ 2 + (Tensor(3) - Tensor(1)) ^ 2) / 6 + Tensor(4) * Tensor(4) + Tensor(5) * Tensor(5) + Tensor(6) * Tensor(6)
        Dim t As Double = (1 / Sqrt3) * Sqrt((Tensor(1) - Tensor(2)) ^ 2 + (Tensor(2) - Tensor(3)) ^ 2 + (Tensor(3) - Tensor(1)) ^ 2 + 6 * Tensor(4) ^ 2 + 6 * Tensor(5) ^ 2 + 6 * Tensor(6) ^ 2)

        Dim DS1 As Double = Tensor(1) - SigmaM
        Dim DS2 As Double = Tensor(2) - SigmaM
        Dim DS3 As Double = Tensor(3) - SigmaM
        Dim d3 As Double = DS1 * DS2 * DS3 - DS1 * Tensor(5) * Tensor(5) - DS2 * Tensor(6) * Tensor(6) - DS3 * Tensor(4) * Tensor(4) + 2 * Tensor(4) * Tensor(5) * Tensor(6)

        Dim dsbar As Double = Sqrt3 * Sqrt(d2)
        Dim J33 As Double = DS1 * DS2 * DS3 - DS1 * Tensor(5) ^ 2 - DS2 * Tensor(6) ^ 2 - DS3 * Tensor(4) ^ 2 + 2 * Tensor(4) * Tensor(5) * Tensor(6)


        SigmaBare = t * Sqrt(1.5)
        Dim V11, V22, V33 As Double
        Dim THETA2 As Double
        If dsbar < 0.0000000001 Then
            THETA = 0
        Else
            Dim sine As Double = -3 * Sqrt3 * d3 / (2 * Sqrt(d2) ^ 3)
            If sine >= 1 Then sine = 1
            If sine < -1 Then sine = -1
            THETA = Asin(sine) / 3
        End If
        V1 = SigmaM + 2 * SigmaBare * Sin(THETA + 2 * PI / 3) / 3
        V2 = SigmaM + 2 * SigmaBare * Sin(THETA) / 3
        V3 = SigmaM + 2 * SigmaBare * Sin(THETA - 2 * PI / 3) / 3

    End Sub



    Public Sub SolveEquaDeg3(a As Double, b As Double, c As Double, d As Double, ByRef S1 As Double,
                                 ByRef S2 As Double, ByRef S3 As Double)
        '
        ' computes the nth real root of the cubic equation
        '
        ' a x^3 + b x^2 + c x + d = 0
        '
        ' =================================================
        Dim xold, f, df, xnew, Err As Double
        Dim iter As Integer
        xold = 1
        iter = 0
        Do
            iter = iter + 1
            f = a * xold ^ 3 + b * xold ^ 2 + c * xold + d
            df = 3 * a * xold ^ 2 + 2 * b * xold + c
            xnew = xold - f / df
            Err = xnew - xold
            xold = xnew
        Loop While (iter < 1000) And (Abs(Err) > 0.000001)
        Dim aa, bb, Real, Disc, S11, S22, S33 As Double
        aa = b / a
        bb = c / a
        Real = -(aa + xnew) / 2
        Disc = (-3 * xnew ^ 2 - 2 * aa * xnew + aa ^ 2 - 4 * bb)
        Disc = Abs(Disc)
        S11 = xnew
        S22 = Real + Disc ^ (1 / 2) / 2
        S33 = Real - Disc ^ (1 / 2) / 2
        S1 = Max(Max(S11, S22), S33)
        S3 = Min(Min(S11, S22), S33)
        If Abs(S11 - S1) > 0.0001 Or Abs(S11 - S3) > 0.0001 Then
            S2 = S11
            Exit Sub
        End If
        If Abs(S22 - S1) > 0.0001 Or Abs(S22 - S3) > 0.0001 Then
            S2 = S22
            Exit Sub
        End If
        If Abs(S33 - S1) > 0.0001 Or Abs(S33 - S3) > 0.0001 Then
            S2 = S33
            Exit Sub
        End If
    End Sub
    Public Function VecAdd(V1() As Double, V2() As Double, Optional Sign As Integer = 1) As Double()
        If V1.Count <= 0 Or IsNothing(V1) Or IsNothing(V2) Or V1.Count <> V2.Count Then

            Return Nothing
        Else
            Dim S(V1.Count - 1) As Double
            For i = 0 To V1.Count - 1
                S(i) = V1(i) + Sign * V2(i)
            Next
            Return S

        End If
    End Function
    Public Function MatriceMatriceAdd(Mtrice1(,) As Double, Mtrice2(,) As Double, Optional Sign As Integer = 1, Optional Depart As Integer = 0) As Double(,)
        Dim k As Integer = Mtrice1.GetLength(0)
        Dim l As Integer = Mtrice1.GetLength(1)

        If k <= 0 Or l <= 0 Or IsNothing(Mtrice1) Or IsNothing(Mtrice2) Then
            Return Nothing
        Else
            Dim Resulat(k - 1, l - 1) As Double
            For i = Depart To k - 1
                For j = Depart To l - 1
                    Resulat(i, j) = Mtrice1(i, j) + Sign * Mtrice2(i, j)
                Next
            Next
            Return Resulat
        End If
    End Function
    Public Function MatriceVecteurMultipe(Mtrice(,) As Double, Vecteur() As Double, Optional Depart As Integer = 0) As Double()
        Dim k As Integer = Mtrice.GetLength(0)
        Dim l As Integer = Vecteur.Count

        If k <= 0 Or l <= 0 Or IsNothing(Mtrice) Or IsNothing(Vecteur) Then
            Return Nothing
        Else

            Dim Resulat(k - 1) As Double
            For i = Depart To k - 1
                Dim x As Double = 0
                For j = Depart To l - 1
                    x += Mtrice(i, j) * Vecteur(j)
                Next
                Resulat(i) = x
            Next
            Return Resulat
        End If
    End Function
    Public Function ScaleVectorMultipe(Scale As Double, Vector() As Double, Optional Depart As Integer = 0) As Double()
        Dim k As Integer = Vector.Count
        If k <= 0 Or IsNothing(Vector) Then
            Return Nothing
        Else
            Dim Resulat(k - 1) As Double
            For i = Depart To k - 1
                Resulat(i) = Vector(i) * Scale
            Next
            Return Resulat
        End If
    End Function
    Public Function ScaleMatrixMultipe(Scale As Double, Matrice(,) As Double, Optional Depart As Integer = 0) As Double(,)
        Dim k As Integer = Matrice.GetLength(0)
        Dim l As Integer = Matrice.GetLength(1)
        If k <= 0 Or IsNothing(Matrice) Then
            Return Nothing
        Else
            Dim Resulat(k - 1, l - 1) As Double
            For i = Depart To k - 1
                For j = Depart To l - 1
                    Resulat(i, j) = Matrice(i, j) * Scale
                Next
            Next
            Return Resulat
        End If
    End Function
    Public Function VecteurVecteurMultipe(Vecteur1() As Double, Vecteur2() As Double, Optional Depart As Integer = 0) As Double
        Dim k As Integer = Vecteur2.Count
        Dim l As Integer = Vecteur1.Count

        If k <= 0 Or l <= 0 Or IsNothing(Vecteur1) Or IsNothing(Vecteur2) Or k <> l Then
            Return Nothing
        Else


            Dim x As Double = 0

            For i = Depart To l - 1
                x += Vecteur1(i) * Vecteur2(i)
            Next
            Return x
        End If
    End Function
    Public Function Convergence(V1() As Double, ByRef V2() As Double, Tol As Double, Iter As Integer) As Boolean
        Dim Iconv As Boolean = True

        Dim Big As Double = 0
        Dim N As Integer = V2.Count - 1
        For i = 0 To N
            If Abs(V1(i)) > Big Then Big = Abs(V1(i))
        Next
        For i = 0 To N
            If (Abs(V1(i) - V2(i)) / Big) > Tol Then Iconv = False
            V2(i) = V1(i)
        Next

        If Iter = 1 Then Iconv = False
        Return Iconv
    End Function

    Public Function Convergence(V1 As Double, ByRef V2 As Double, Tol As Double) As Boolean
        Dim Iconv As Boolean
        Iconv = False
        If Abs(V1 - V2) < Tol Then Iconv = True
        Return Iconv
    End Function

End Class




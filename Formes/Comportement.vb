Public Class Comportement
    Public Comportement As Integer = 0
    Public Approch As Integer = 1
    Public Ev, V, Fb0_Fc0, Kc, PsiDegre, Fck, Excent, Rhou As double
    Public Comprission As List(Of Base.DoublePoint)
    Public Tension As List(Of Base.DoublePoint)
    Private Sub Comportement_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ComboBox1.Items.Clear()
        ComboBox1.Items.Add("Linear material properties")
        ComboBox1.Items.Add("Concrete Damaged Plasticity")

        ComboBox2.Items.Clear()
        ComboBox2.Items.Add("Alfarah Methode")
        ComboBox2.Items.Add("Bakhti Methode")
        ComboBox2.Items.Add("User Curve")
        ComboBox2.SelectedIndex = Approch

        DataGridView1.ColumnCount = 2
        DataGridView1.Columns(0).Name = "Stress"
        DataGridView1.Columns(0).Width = 90
        DataGridView1.Columns(1).Name = "Strain"
        DataGridView1.Columns(1).Width = 90

        If Not IsNothing(Comprission) Then
            For i = 0 To Comprission.Count - 1
                DataGridView1.Rows.Add(Comprission.Item(i).y, Comprission.Item(i).x)
            Next
        End If

        DataGridView2.ColumnCount = 2
        DataGridView2.Columns(0).Name = "Stress"
        DataGridView2.Columns(0).Width = 90
        DataGridView2.Columns(1).Name = "Strain"
        DataGridView2.Columns(1).Width = 90

        If Not IsNothing(Tension) Then
            For i = 0 To Tension.Count - 1
                DataGridView2.Rows.Add(Tension.Item(i).y, Tension.Item(i).x)
            Next
        End If
        Approch = ComboBox2.SelectedIndex
        TabControl1.Height = 182
        Button1.Top = 226
        Height = 300
        TabControl2.Visible = False
        If Approch = 2 Then
            TabControl1.Height = 338
            Button1.Top = 383
            Height = 454
            TabControl2.Visible = True
        End If

        If Comportement >= 0 Then ComboBox1.SelectedIndex = Comportement
        If Comportement = 0 Then
            TextBox1.Text = Ev.ToString
            TextBox2.Text = V.ToString
        End If

        TextBox3.Text = Fb0_Fc0.ToString
        TextBox4.Text = PsiDegre.ToString
        TextBox5.Text = Excent.ToString
        TextBox6.Text = Kc.ToString
        TextBox7.Text = Fck.ToString
        TextBox8.Text = V.ToString
    End Sub


    Private Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox1.SelectedIndexChanged
        TextBox1.Visible = False
        TextBox2.Visible = False
        Label1.Visible = False
        Label2.Visible = False
        If ComboBox1.SelectedIndex = 0 Then
            TabControl1.Visible = False
            TextBox1.Visible = True
            TextBox2.Visible = True
            Label1.Visible = True
            Label2.Visible = True
            Button1.Top = 106
            Height = 183
        End If
        If ComboBox1.SelectedIndex = 1 Then
            TabControl1.Visible = True
            TabControl1.Height = 182
            Button1.Top = 226
            Height = 300
            TabControl2.Visible = False
            If Approch = 2 Then
                TabControl1.Height = 338
                Button1.Top = 383
                Height = 454
                TabControl2.Visible = True
            End If
        End If
        Comportement = ComboBox1.SelectedIndex


    End Sub

    Private Sub ComboBox2_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox2.SelectedIndexChanged
        If ComboBox1.SelectedIndex <> 0 Then
            Approch = ComboBox2.SelectedIndex
            TabControl1.Height = 182
            Button1.Top = 226
            Height = 300
            TabControl2.Visible = False

            If Approch = 2 Then
                TabControl1.Height = 338
                Button1.Top = 383
                Height = 454
                TabControl2.Visible = True

            End If
        End If
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click

    End Sub
End Class
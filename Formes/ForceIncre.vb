Public Class ForceIncre
    Public TypeForce As Integer
    Public Increments As New List(Of double)
    Public Valeur As double
    Public ChargeRepartie As Boolean
    Private Sub ForceIncre_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Button1.Left = 12
        TextBox1.Left = 12
        DataGridView1.Left = 12
        DataGridView1.Top = 37
        TextBox1.Top = 37
        ComboBox1.Items.Clear()
        ComboBox1.Items.Add("Fixe")
        ComboBox1.Items.Add("Increment")
        If TypeForce >= 0 Then ComboBox1.SelectedIndex = TypeForce

    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If TypeForce = 0 Then
            Valeur = TextBox1.Text
            ChargeRepartie = CheckBox1.Checked
        End If
        If TypeForce = 1 Then
            Increments.Clear()
            For i = 0 To DataGridView1.Rows.Count - 1
                If Convert.ToDouble(DataGridView1.Rows.Item(i).Cells(0).Value) <> 0 Then
                    Increments.Add(Convert.ToDouble(DataGridView1.Rows.Item(i).Cells(0).Value))
                End If
            Next
        End If
        Close()
    End Sub

    Private Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox1.SelectedIndexChanged
        TypeForce = ComboBox1.SelectedIndex
        If ComboBox1.SelectedIndex = 0 Then
            Text = "Fixe Value"
            CheckBox1.Visible = True
            DataGridView1.Visible = False
            TextBox1.Visible = True
            Width = 224
            Height = 174
            Button1.Top = 93
            TextBox1.Text = Valeur
            CheckBox1.Checked = ChargeRepartie
        End If
        If ComboBox1.SelectedIndex = 1 Then
            Text = "Load Increments"
            DataGridView1.Visible = True
            TextBox1.Visible = False
            CheckBox1.Visible = False
            Width = 224
            Height = 440
            Button1.Top = 360
            DataGridView1.ColumnCount = 1
            DataGridView1.Columns(0).Name = "Load"
            DataGridView1.Columns(0).Width = 90
            DataGridView1.ColumnHeadersDefaultCellStyle.Tag = "N"


            For i = 0 To Increments.Count - 1
                DataGridView1.Rows.Add(Increments.Item(i))
            Next

        End If
    End Sub
End Class
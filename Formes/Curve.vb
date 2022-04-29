Public Class Curve
    Private Sub Curve_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ComboBox1.Items.Clear()
        ' ComboBox1.Items.Add("Loads-Displacement")
        ComboBox1.Items.Add("Comprissive: Stress-Strain")
        ComboBox1.Items.Add("Damage-Parameter Curve")
        ComboBox1.Items.Add("Tensile: Stress-Strain")
        ComboBox1.Items.Add("Comprissive CDP Model Curve: Bakhti")
        ComboBox1.Items.Add("Comprissive Damage Parameter Curve: Bakhti")
        ComboBox1.Items.Add("Tensile CDP Model Curve: Bakhti")
        ComboBox1.Items.Add("Tensile CDP Damage Parameter Curve: Bakhti")
        ComboBox1.Items.Add("Comprissive CDP Model Curve: Alfarah")
        ComboBox1.Items.Add("Comprissive Damage Parameter Curve: Alfarah")
        ComboBox1.Items.Add("Tensile CDP Model Curve: Alfarah")
        ComboBox1.Items.Add("Tensile Damape Parameter Curve: Alfarah")
        ComboBox1.Items.Add("Comprissive Damage-Parameter Curve")
        ComboBox1.SelectedIndex = 0
    End Sub

    Private Sub PictureBox1_Paint(sender As Object, e As PaintEventArgs) Handles PictureBox1.Paint

        DrawingGraph(PictureBox1.Width, PictureBox1.Height, IndexDraw(ComboBox1.Text), e.Graphics, 2)

    End Sub

    Private Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox1.SelectedIndexChanged
        PictureBox1.Invalidate()
    End Sub

    Private Sub NumericUpDown1_ValueChanged(sender As Object, e As EventArgs) Handles NumericUpDown1.ValueChanged
        PictureBox1.Invalidate()
    End Sub

    Private Sub NumericUpDown2_ValueChanged(sender As Object, e As EventArgs) Handles NumericUpDown2.ValueChanged
        PictureBox1.Invalidate()
    End Sub

    Private Sub NumericUpDown3_ValueChanged(sender As Object, e As EventArgs) Handles NumericUpDown3.ValueChanged
        PictureBox1.Invalidate()
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        PictureExport.ShowDialog()
        PictureExport.Dispose()
    End Sub
End Class
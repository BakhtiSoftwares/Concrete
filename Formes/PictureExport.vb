Public Class PictureExport
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim W As double = Decoval(TextBox1.Text)
        Dim h As double = Decoval(TextBox2.Text)
        Dim ppi As double = Decoval(TextBox3.Text)
        Dim Epai As double = Decoval(TextBox4.Text)
        W = W * ppi / 2.54
        h = h * ppi / 2.54
        SaveBitmap(Int(W), Int(h), IndexDraw(Curve.ComboBox1.Text), Epai)
    End Sub
End Class
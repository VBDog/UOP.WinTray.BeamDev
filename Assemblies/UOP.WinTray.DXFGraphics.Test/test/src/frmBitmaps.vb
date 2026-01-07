
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports dxfGraphicsNET.dxfGraphics

Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.PowerPacks.Printing.Compatibility.VB6.ColorConstants
Imports System.IO
Imports System.Windows.Forms

Imports System.Reflection


Public Class frmBitmaps

    Dim myBitmap As dxfBitmap

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click

        Test2()
        'Test1()


    End Sub

    Public Sub Test1()
        Dim g As Graphics = Me.CreateGraphics()

        Dim bmap As New System.Drawing.Bitmap(Me.Width, Me.Height, g)
        'Me.DrawToBitmap(bmap, New Drawing.Rectangle(0, 0, bmap.Width, bmap.Height))

        If myBitmap Is Nothing Then
            myBitmap = New dxfBitmap(bmap)
        End If

        'myBitmap.CopyFromClipboard()

        Dim aHBMP As IntPtr = myBitmap.hBMP
        Dim bytes(0) As Byte
        'Dim pstep As Integer
        'Dim k As Integer

        'If myBitmap.GetBytes(bytes, pstep) Then
        '    For i As Integer = 0 To bytes.Length - 1 Step pstep
        '        k += 1
        '        Debug.Print(k.ToString & ") R:" & bytes(i + 2) & " G:" & bytes(i + 1) & " B:" & bytes(i))
        '        '    .Blue = rBytes(i)
        '        '    '        .Green = rBytes(i + 1)
        '        '    '        .Red = rBytes(i + 2)

        '        If k > 1 Then Exit For
        '    Next i

        'End If

        ' System.Windows.Forms.Clipboard.SetImage(myBitmap.Bitmap)       


        'g.DrawImage(myBitmap.Bitmap, New Point(0, 0))

        'THIS PERSISTS!
        PictureBox2.Image = myBitmap.Image


        'THIS DOES NOT PERSIST!

        'myBitmap.Stretch(0.5)

        myBitmap.StretchToControl(PictureBox1.Handle)

        'g.ReleaseHdc()
        '
        'System.Windows.Forms.Clipboard.SetImage(myBitmap.Bitmap)

        PictureBox1.Invalidate()
        PictureBox1.Refresh()

        'MsgBox(gptr.ToString)


        myBitmap.Dispose()
        myBitmap = Nothing

    End Sub
    Public Sub Test4()

        Dim map As dxfBitmap
        Dim bmap As dxfBitmap

        Try

            map = New dxfBitmap(PictureBox1.Width, PictureBox1.Height)
            map.CopyFromControl(PictureBox1.Handle)
            bmap = New dxfBitmap(map)

            bmap.Flip(True)
            bmap.CopyToClipBoard()


            Dim g As Graphics = PictureBox2.CreateGraphics
            g.Clear(Color.White)
            'this doesnt persist
            bmap.DrawToDevice(PictureBox2.Handle, dxxColorModes.GreyScales)

            g.ReleaseHdc()


        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Public Sub Test2()


        Dim cmap As dxfBitmap = Nothing
        Dim g As Graphics = Nothing
        Try
            ' MsgBox(dxfUtilities.ReadINI_String("", "PropertyViewer", "DimOverrides"))
            myBitmap = New dxfBitmap(PictureBox1.Width, PictureBox1.Height)

            'myBitmap = New dxfBitmap("C:\junk\Bitmap.bmp")

            'Me.Enabled = False

            myBitmap.SetBackColor(Color.Red, True)
            PictureBox1.Image = myBitmap.Image
            myBitmap.AddBorder(Color.LightGray, 12)
            PictureBox1.Image = myBitmap.Image
            myBitmap.ReplaceColor(Color.Red, Color.Chocolate)
            PictureBox1.Image = myBitmap.Image
            GC.Collect()

            g = PictureBox2.CreateGraphics()
            g.FillRectangle(New SolidBrush(Color.White), PictureBox2.ClientRectangle)
            g.Dispose()
            PictureBox2.Image = Nothing
            PictureBox2.Refresh()

            'cmap = dxfUtilities.Colors.ACLPallet(25, Color.Red)

            cmap = myBitmap.Clone

            cmap.ReplaceColor(Color.Chocolate, Color.AliceBlue)
            cmap.StretchToControl(PictureBox2.Handle, 0.5,, dxxRectangularAlignments.BottomLeft,,, dxxColorModes.GreyScales)

            ''cmap.Resize(0.5 * cmap.Width, cmap.Height)
            'cmap.AddBorder(Color.Black, 12, True, False, True, True)
            'cmap.Flip(True)
            'cmap.ConvertTo(dxxColorModes.dxfColorModeGreyScales)
            'cmap.DrawToDevice(PictureBox2.Handle, dxxRectangularAlignments.MiddleCenter)
            'cmap.CheckerBoard(20, Color.BurlyWood.ToArgb)
            'cmap.FloodFill(Color.Gold)
            'cmap.DrawToDevice(PictureBox2.Handle, dxxRectangularAlignments.MiddleCenter)

            'g = Graphics.FromHwnd(PictureBox2.Handle)

            'cmap.DrawTo(CLng(g.GetHdc),, 100, 100)
            'cmap.Stretch(1.2)
            'cmap.DrawToControl(PictureBox2)
            'cmap.StretchTo(PictureBox2.Handle, 0)

            'dxfUtilities.ControlToClipBoard(PictureBox2)
            'System.Windows.Forms.Clipboard.Clear()
            'cmap.CopyToClipBoard()



            'PictureBox1.BackgroundImage = myBitmap.Bitmap
            'g.Dispose()


            'g = Graphics.FromHwnd(PictureBox2.Handle)
            'cmap = myBitmap.Clone
            'cmap.ReplaceColor(Color.Chocolate, Color.Firebrick)
            'g.DrawImage(cmap.Bitmap, New Point(0, 0))

            'myBitmap.Bitmap.Save("C:\junk\Bitmap.bmp")
        Catch ex As Exception
            MsgBox(ex.Message)
        Finally
            If g IsNot Nothing Then g.Dispose()

            'If myBitmap IsNot Nothing Then myBitmap.Dispose()
            If cmap IsNot Nothing Then cmap.Dispose()


        End Try

        GC.Collect()

        'Me.Enabled = True
        'Me.Refresh()

    End Sub

    Private Sub Test3()
        Dim aImage As New dxfImage
        Dim aNums(0 To 9) As Integer
        Dim bNums(0 To 9) As Integer
        'Dim cNums() As Integer

        Try


            'bNums(9) = 22
            'cNums = aNums.Concat(bNums).ToArray
            'MsgBox(cNums(19).ToString)

            aImage.Display.SetDevice(PictureBox2)
            'MsgBox(aImage.Display.DeviceDC)
            aImage.Draw.aLine(New dxfVector(0, 0), New dxfVector(10, 10),, dxxColors.Green)
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        Finally
            aImage.Display.ZoomExtents()
            aImage.Bitmap.DrawToDevice(PictureBox2.Handle)
            aImage.Bitmap.CopyToClipBoard()
            aImage.Dispose()
            GC.Collect()
        End Try

    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click



    End Sub

    Private Sub PictureBox1_Paint(sender As Object, e As PaintEventArgs) Handles PictureBox1.Paint
        If myBitmap IsNot Nothing Then
            PictureBox1.Image = myBitmap.Image
        End If
    End Sub

    Private Sub dxfGraphicsTestForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        If myBitmap IsNot Nothing Then myBitmap.Dispose()
        myBitmap = Nothing
    End Sub

    Private Sub dxfGraphicsTestForm_ControlRemoved(sender As Object, e As ControlEventArgs) Handles Me.ControlRemoved

    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Test4()
    End Sub

    Private Sub PictureBox1_Click(sender As Object, e As EventArgs) Handles PictureBox1.Click

    End Sub

    Private Sub frmBitmaps_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        lblWorld.Text = ""
        lblPXs.Text = ""
        txtPic2.Text = ""

        Dim g As Graphics = PictureBox2.CreateGraphics()


        txtPic2.Text = "W = " & Format(PictureBox2.Width / g.DpiX, "0.00") & "'' H = " & Format(PictureBox2.Height / g.DpiY, "0.00") & "''"

        g.Dispose()

    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        Dim g As Graphics = PictureBox2.CreateGraphics
        Dim gpath As New GraphicsPath(FillMode.Winding)
        Dim Pts(2) As PointF
        Dim Cds(2) As Byte
        g.Clear(Color.White)
        Pts(0) = New PointF(20, 20)
        Pts(1) = New PointF(20, 200)
        Pts(2) = New PointF(100, 200)

        Cds(0) = PathPointType.Start
        Cds(1) = PathPointType.Line
        Cds(2) = PathPointType.Line

        gpath.AddPath(New GraphicsPath(Pts, Cds), False)

        Pts(0) = New PointF(200, 20)
        Pts(1) = New PointF(200, 200)
        Pts(2) = New PointF(300, 200)

        gpath.AddPath(New GraphicsPath(Pts, Cds), False)
        g.DrawPath(New Pen(Color.Black), gpath)

        g.Dispose()

        'TryIt()

    End Sub

    Private Sub TryIt()

        Dim pbsz As System.Drawing.Size = PictureBox2.Size
        Dim aBMP As New System.Drawing.Bitmap(PictureBox2.Width, PictureBox2.Height)
        Dim g As Graphics = Graphics.FromImage(aBMP)

        Dim p As Pen
        Dim rec As Rectangle
        Dim sz As System.Drawing.Size
        Dim p1 As New PointF(0, 0)
        Dim p2 As New PointF(0, 0)
        Dim baseRec As New Drawing.Size
        Dim sf As Single = g.DpiX
        Dim sngSz As New System.Drawing.SizeF(PictureBox2.Width / sf, PictureBox2.Height / sf)

        Try


            g.PageScale = 1 / g.DpiX

            g.FillRectangle(Brushes.Blue, New Rectangle(New Point(0, 0), PictureBox2.Size))

            g.ScaleTransform(sf / 1, -sf / 1)

            baseRec.Width = CInt(sngSz.Width)
            baseRec.Height = CInt(sngSz.Height)

            g.TranslateTransform(baseRec.Width / 2, -baseRec.Height / 2)

            p = New Pen(Color.Black)
            p.Width = 1 / sf
            sz = New Drawing.Size(baseRec.Width / 4, baseRec.Width / 4)


            rec = New Rectangle(New Point(-sz.Width / 2, -sz.Height / 2), sz)

            g.DrawEllipse(p, rec)

            p2.X = baseRec.Width / 2
            p2.Y = baseRec.Height / 2
            g.DrawLine(p, p1, p2)


            p.Dispose()

            'For i As Integer = 0 To aBMP.Width - 1
            '    For j As Integer = 0 To aBMP.Height - 1
            '        aBMP.SetPixel(i, j, Color.Red)

            '    Next
            'Next

            Clipboard.SetImage(aBMP)


            g.Dispose()

            PictureBox2.Image = Clipboard.GetImage()
            '    Beep()
        Catch ex As Exception
            MsgBox(ex)
        Finally
            aBMP.Dispose()
        End Try

    End Sub

    Private Sub PictureBox2_Click(sender As Object, e As EventArgs) Handles PictureBox2.Click

    End Sub

    Private Sub PictureBox2_MouseMove(sender As Object, e As MouseEventArgs) Handles PictureBox2.MouseMove
        lblPXs.Text = "X:" & e.X & " Y:" & e.Y

        Dim g As Graphics = PictureBox2.CreateGraphics()
        Dim pts(0) As Drawing.PointF
        pts(0).X = e.X
        pts(0).Y = e.Y

        g.TransformPoints(CoordinateSpace.Device, CoordinateSpace.World, pts)

        lblWorld.Text = "X:" & pts(0).X & " Y:" & pts(0).Y

        g.Dispose()


    End Sub
End Class

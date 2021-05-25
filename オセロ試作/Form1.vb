Public Class Form1
    Dim Field(7, 7) As Integer '盤面全体の状況を配列でもつ。0：空白　1：黒　-1：白
    Dim PlayerTurn As Boolean = True '現在どちらのターンかを記録する
    'スタートボタンの処理
    Private Sub Start_Button_Click(sender As Object, e As EventArgs) Handles Start_Button.Click
        FirstSetting()
    End Sub
    '盤面をリセットし、中心の４マスに石を並べる
    Private Sub FirstSetting()
        For x = 0 To 7 Step 1
            For y = 0 To 7 Step 1
                Field(x, y) = 0
            Next
        Next
        Refresh()
        Create_Grid()
        Field(4, 3) = 1
        Field(3, 4) = 1
        Field(3, 3) = -1
        Field(4, 4) = -1
    End Sub
    '盤面に線を描画する
    Private Sub Create_Grid()
        Dim grph As System.Drawing.Graphics = Othello_Field.CreateGraphics
        Dim blackPen As New Pen(Color.Black, 10)
        grph.DrawLine(blackPen, 0, 0, 0, 400)
        grph.DrawLine(blackPen, 0, 0, 400, 0)
        grph.DrawLine(blackPen, 400, 400, 400, 0)
        grph.DrawLine(blackPen, 400, 400, 0, 400)
        For x = 0 To 400 Step 50
            grph.DrawLine(Pens.Black, x, 0, x, 400)
            grph.DrawLine(Pens.Black, 0, x, 400, x)
        Next
    End Sub
    '毎フレーム盤面を更新する
    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Create_Field()
    End Sub
    '盤面に石を描画する
    Private Sub Create_Field()
        Dim grph As System.Drawing.Graphics = Othello_Field.CreateGraphics
        For x = 0 To 7 Step 1
            For y = 0 To 7 Step 1
                If Field(x, y) > 0 Then
                    grph.FillEllipse(Brushes.Black, x * 50, y * 50, 50, 50)
                ElseIf Field(x, y) < 0 Then
                    grph.FillEllipse(Brushes.White, x * 50, y * 50, 50, 50)
                End If
            Next
        Next
    End Sub
    '石を置いた座標を得て黒石を置く
    Private Sub Othello_Field_Click(sender As Object, e As MouseEventArgs) Handles Othello_Field.MouseDown, Othello_Field.Click
        If PlayerTurn = False Then
            Return
        End If
        Dim grph As System.Drawing.Graphics = Othello_Field.CreateGraphics
        Dim x, y As Single
        x = Math.Floor((e.X) / 50)
        y = Math.Floor((e.Y) / 50)

        If Turn_Stone_Simulator(x, y, 1) = False Then
            Undo_Simulator()
            Debug.WriteLine("Undo")
        ElseIf Math.Abs(Field(x, y)) > 0 Then
            Undo_Simulator()
        Else
            Field(x, y) = 1
            grph.FillEllipse(Brushes.Black, x * 50, y * 50, 50, 50)
            Turn_Stone()
            PlayerTurn = False
        End If
    End Sub

    '石を置いた座標を得て白石を置く
    Private Sub White_Othello_Field_Click(sender As Object, e As MouseEventArgs) Handles Othello_Field.MouseDown
        If PlayerTurn = True Then
            Return
        End If
        Dim grph As System.Drawing.Graphics = Othello_Field.CreateGraphics
        Dim x, y As Single
        x = Math.Floor((e.X) / 50)
        y = Math.Floor((e.Y) / 50)

        If Turn_Stone_Simulator(x, y, -1) = False Then
            Undo_Simulator()
            Debug.WriteLine("Undo")
        ElseIf Math.Abs(Field(x, y)) > 0 Then
            Undo_Simulator()
        Else
            Field(x, y) = -1
            grph.FillEllipse(Brushes.White, x * 50, y * 50, 50, 50)
            Turn_Stone()
            PlayerTurn = True
        End If
    End Sub

    '配列のうちひっくり返せるマスの数値を二倍する関数。呼んだならばUndo_Simulator関数かTurn_Stone関数を必ず呼ぶ。ひっくり返せる石がある場合はTrueを返す
    Private Function Turn_Stone_Simulator(x As Single, y As Single, Optional z As Integer = 100) As Boolean
        Dim keypoint As Integer
        Dim dx, dy As Integer
        Dim serch_x, serch_y As Integer
        Dim result As Integer = 1
        Dim isTurn As Boolean = True
        Dim a, b As Integer
        If z = 100 Then
            keypoint = Field(x, y)
        Else
            keypoint = z
        End If

        For dx = -1 To 1 Step 1
            For dy = -1 To 1 Step 1
                result = 1
                serch_x = x
                serch_y = y
                While result = 1
                    serch_x = serch_x + dx
                    serch_y = serch_y + dy
                    result = isTurn_Stone(keypoint, serch_x, serch_y)
                    If result = 0 Then
                        While Math.Abs(Field(serch_x - dx, serch_y - dy)) = 2
                            serch_x = serch_x - dx
                            serch_y = serch_y - dy
                            Field(serch_x, serch_y) = Field(serch_x, serch_y) / 2

                        End While
                    End If
                End While
            Next
        Next
        isTurn = False
        For a = 0 To 7 Step 1
            For b = 0 To 7 Step 1
                If Math.Abs(Field(a, b)) > 1 Then
                    isTurn = True
                End If
            Next
        Next
        Return isTurn
    End Function
    '絶対値で2の入っているマスの数値を半分にする
    Private Sub Undo_Simulator()
        For a = 0 To 7 Step 1
            For b = 0 To 7 Step 1
                If Math.Abs(Field(a, b)) > 1 Then
                    Field(a, b) = (Field(a, b) / 2)
                End If
            Next
        Next
    End Sub
    '絶対値で2の入っているマスの色を変えて数値を半分にする
    Private Sub Turn_Stone()
        For a = 0 To 7 Step 1
            For b = 0 To 7 Step 1
                If Math.Abs(Field(a, b)) > 1 Then
                    Field(a, b) = (Field(a, b) / 2) * -1
                End If
            Next
        Next
    End Sub
    '指定された座標のマスとkeypointのマスを比較する。空、枠外：0　異色：１　同色：２
    Private Function isTurn_Stone(keypoint As Integer, x As Integer, y As Integer) As Integer
        If x >= 8 Or y >= 8 Or x <= -1 Or y <= -1 Then
            Return 0
        ElseIf Field(x, y) = 0 Then
            Return 0
        ElseIf keypoint = Field(x, y) * -1 Then
            Field(x, y) = Field(x, y) * 2
            Return 1
        ElseIf keypoint = Field(x, y) Then
            Return 2
        End If
        Return 0
    End Function
End Class

Public Class Form1
    Dim Field(7, 7) As Integer '盤面全体の状況を配列でもつ。0：空白　1：黒　-1：白
    Dim SimField(7, 7) As Integer '盤面をシミュレートするための仮想盤。0：空白　1：黒　-1：白
    Dim PlayerTurn As Boolean = True '現在どちらのターンかを記録する
    Dim IsGameStart As Boolean = False 'ゲームの開始を判定
    Dim PassCount As Boolean = False 'trueの時にパスされるとゲームセット。falseの時にパスされると次回のターンまでtrueになる
    Dim scoreboard = {{120, -20, 20, 5, 5, 20, -20, 120},
                       {-20, -40, -5, -5, -5, -5, -40, -20},
                       {20, -5, 15, 3, 3, 15, -5, 20},
                       {5, -5, 3, 3, 3, 3, -5, 5},
                       {5, -5, 3, 3, 3, 3, -5, 5},
                       {20, -5, 15, 3, 3, 15, -5, 20},
                       {-20, -40, -5, -5, -5, -5, -40, -20},
                       {120, -20, 20, 5, 5, 20, -20, 120}} '評価値表


    '-------------------------------ゲームの基本処理------------------------


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
    '毎フレーム盤面を更新する
    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Create_Field()
    End Sub
    'SimFieldの配列を0で満たす。SimFieldは様々な関数で使う上にスコープが広いため、使用する関数内では問題がない限りこの関数を呼んで初期化する
    Private Sub SimFieldReset()
        For a = 0 To 7 Step 1
            For b = 0 To 7 Step 1
                SimField(a, b) = 0
            Next
        Next
    End Sub
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
        SimFieldReset()
        Refresh()
        Create_Grid()
        Field(4, 3) = 1
        Field(3, 4) = 1
        Field(3, 3) = -1
        Field(4, 4) = -1
        PlayerTurn = True
        IsGameStart = True
    End Sub
    '結果を表示しゲームを終了する
    Private Sub EndGame()
        Dim myscore As Integer = 0
        Dim enemyscore As Integer = 0
        MsgBox("ゲーム終了")
        For x = 0 To 7 Step 1
            For y = 0 To 7 Step 1
                If Field(x, y) = 0 Then
                ElseIf Field(x, y) = 1 Then
                    myscore = myscore + 1
                ElseIf Field(x, y) = -1 Then
                    enemyscore = enemyscore + 1
                End If
            Next
        Next
        If myscore < enemyscore Then
            MsgBox(myscore & "-" & enemyscore & "であなたの負けです")
        ElseIf myscore > enemyscore Then
            MsgBox(myscore & "-" & enemyscore & "であなたの勝ちです")
        ElseIf myscore = enemyscore Then
            MsgBox(myscore & "-" & enemyscore & "で引き分けです")
        End If
        Application.Restart()
    End Sub


    '--------------------盤面のシミュレーションに関する処理---------


    'ひっくり返せるマスをSimFieldに記録する関数。ひっくり返せる石がある場合はTrueを返す
    Private Function Turn_Stone_Simulator2(x As Single, y As Single, Optional z As Integer = 100) As Boolean
        Dim keypoint As Integer
        Dim dx, dy As Integer
        Dim serch_x, serch_y As Integer
        Dim result As Integer = 1
        Dim isTurn As Boolean = True
        Dim a, b As Integer
        'ｚに数値がなければField(x,y)の座標の石の色を参照する
        If z = 100 Then
            keypoint = Field(x, y)
        Else
            keypoint = z
        End If
        'dx,dyを-1~+1まで変化させ8方向のひっくり返せる石を探索する
        For dx = -1 To 1 Step 1
            For dy = -1 To 1 Step 1
                If dx = 0 And dy = 0 Then
                    dy = dy + 1
                End If
                result = 1
                serch_x = x
                serch_y = y
                While result = 1
                    serch_x = serch_x + dx
                    serch_y = serch_y + dy
                    result = isTurn_Stone2(keypoint, serch_x, serch_y)
                    'ひっくり返せない場所のSimFieldを部分的に0に戻す
                    If result = 0 Then
                        While Math.Abs(SimField(serch_x - dx, serch_y - dy)) <> 0
                            serch_x = serch_x - dx
                            serch_y = serch_y - dy
                            SimField(serch_x, serch_y) = 0
                        End While
                    End If
                End While
            Next
        Next
        isTurn = False
        'ひっくり返せるマスがあったかどうか判定
        For a = 0 To 7 Step 1
            For b = 0 To 7 Step 1
                If Math.Abs(SimField(a, b)) = 1 Then
                    isTurn = True
                End If
            Next
        Next
        Return isTurn
    End Function
    '指定された座標のマスとkeypointのマスを比較する。空、枠外：0　異色：１　同色：２
    Private Function isTurn_Stone2(keypoint As Integer, x As Integer, y As Integer) As Integer
        If x >= 8 Or y >= 8 Or x <= -1 Or y <= -1 Then
            Return 0
        ElseIf Field(x, y) = 0 Then
            Return 0
        ElseIf keypoint = Field(x, y) * -1 Then
            SimField(x, y) = keypoint
            Return 1
        ElseIf keypoint = Field(x, y) Then
            Return 2
        End If
        Return 0
    End Function
    'SimFieldに記録されているひっくり返せる石をひっくり返す
    Private Sub Turn_Stone2()
        For a = 0 To 7 Step 1
            For b = 0 To 7 Step 1
                If Math.Abs(SimField(a, b)) = 1 Then
                    Field(a, b) = SimField(a, b)
                End If
            Next
        Next
    End Sub


    '---------------------先手番の処理-----------------------


    '石を置いた座標を得て置けるなら黒石を置く
    Private Sub Othello_Field_Click(sender As Object, e As MouseEventArgs) Handles Othello_Field.MouseDown, Othello_Field.Click
        If PlayerTurn = False Then
            Return
        End If
        Dim x, y As Single
        SimFieldReset()
        x = Math.Floor((e.X) / 50)
        y = Math.Floor((e.Y) / 50)
        Pass_checker()
        If Turn_Stone_Simulator2(x, y, 1) = False Then
            hyouka(x, y)
        ElseIf Math.Abs(Field(x, y)) > 0 Then
        Else
            Field(x, y) = 1
            Turn_Stone2()
            SimFieldReset()
            PlayerTurn = False
            Create_Field()
            PassCount = False
            AI_Manager2()
        End If
    End Sub


    '--------------------後手番の処理---------------------


    'AI2関数を呼び、返り値の座標に石を置く。その後、プレイヤーが石を置けない時はパスさせる
    Private Sub AI_Manager2()
        If PlayerTurn = True Then
            Exit Sub
        End If
        Dim put_stone_position() As Integer = New Integer() {-1, -1}
        put_stone_position = AI2()
        If put_stone_position(0) = -1 Then
            Exit Sub
        End If
        SimFieldReset()
        Turn_Stone_Simulator2(put_stone_position(0), put_stone_position(1), -1)
        Field(put_stone_position(0), put_stone_position(1)) = -1
        Turn_Stone2()
        PlayerTurn = True
        Pass_checker()
    End Sub
    '評価値を基に白石を置くのにもっとも有効と思われる座標を返す
    Private Function AI2()
        Dim bestposition() As Integer = New Integer() {-1, -1}
        Dim score As Integer
        Dim best_score As Integer = 1000
        Dim score_sum As Integer = 0
        For a = 0 To 7 Step 1
            For b = 0 To 7 Step 1
                SimFieldReset()
                If Math.Abs(Field(a, b)) > 0 Then
                ElseIf Turn_Stone_Simulator2(a, b, -1) = True Then
                    '評価値計算
                    score = hyouka(a, b)
                    '最大値適用
                    If score < best_score Then
                        best_score = score
                        bestposition = {a, b}
                    End If
                End If
            Next
        Next
        '結果を返す
        If bestposition(0) > -1 Then
            PassCount = False
            Return bestposition
        End If

        'Passの処理
        If PassCount = True Then
            PassCount = False
            EndGame()
        End If
        MsgBox("パスされました")
        PassCount = True
        PlayerTurn = True
        'Fieldをきれいにする処理
        Return bestposition
    End Function
    '引数の座標に白石を置いたときの有効度を評価値表を基に算出する
    Private Function hyouka(x As Integer, y As Integer)
        Dim score As Integer = 0
        score = score - scoreboard(x, y)
        For a = 0 To 7 Step 1
            For b = 0 To 7 Step 1
                score = score + SimField(a, b) * scoreboard(a, b)
            Next
        Next
        Return score
    End Function
    '黒石が置ける場所がないときにパスさせる
    Private Sub Pass_checker()
        If PlayerTurn = True And IsGameStart = True Then
            For a = 0 To 7 Step 1
                For b = 0 To 7 Step 1
                    If Field(a, b) = 0 Then
                        If Turn_Stone_Simulator2(a, b, 1) = True Then
                            SimFieldReset()
                            Exit Sub
                        End If
                    End If
                    SimFieldReset()
                Next
            Next
            If PassCount = True Then
                PassCount = False
                EndGame()
            End If
            MsgBox("パスします")
            PassCount = True
            PlayerTurn = False
            AI_Manager2()
        End If
    End Sub
End Class

Public Class MultipleControlTypesSupported
    Sub fld_PreviewKeyDown(sender As Object, e As KeyEventArgs)

        If e.Key = Key.F3 Or (e.Key = Key.F And (Keyboard.IsKeyDown(Key.LeftCtrl) Or Keyboard.IsKeyDown(Key.RightCtrl))) Then
            e.Handled = True
            ApplicationCommands.Find.Execute(Nothing, Me)
        End If
    End Sub

    Sub MenuItem_Click(sender As Object, e As RoutedEventArgs)

        'Ensure the FlowDocumentScrollViewer doesn't have focus, otherwise Find command will go to it.
        Focus()
        ApplicationCommands.Find.Execute(Nothing, Me)
    End Sub
End Class

Public Class Customer
    Dim fname As String
    Dim lname As String

    Public Property FirstName As String
        Get
            Return fname
        End Get
        Set(value As String)
            fname = value
        End Set
    End Property

    Public Property LastName As String
        Get
            Return lname
        End Get
        Set(value As String)
            lname = value
        End Set
    End Property
End Class

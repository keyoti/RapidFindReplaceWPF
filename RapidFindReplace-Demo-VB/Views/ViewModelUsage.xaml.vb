Imports Keyoti.RapidFindReplace.WPF

Public Class ViewModelUsage

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        _searchTextBox.Focus()
    End Sub

    Sub _searchTextBox_KeyUp(sender As Object, e As KeyEventArgs)
        If cb1.IsChecked Then
            Dim viewModel As RapidFindReplaceControlViewModel = CType(_searchTextBox.DataContext, RapidFindReplaceControlViewModel)
            viewModel.FindText(viewModel.FindScope)
        End If

    End Sub

End Class

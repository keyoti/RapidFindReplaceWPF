Class MainWindow 
    Shared Sub RunWindow(selectedItem As Type)
        Dim window As Type = System.Reflection.Assembly.GetExecutingAssembly.GetType(selectedItem.FullName)
        Dim win As Window = CType(Activator.CreateInstance(window), Window)
        win.Topmost = True
        Application.Current.Dispatcher.BeginInvoke(Sub() win.Show())
    End Sub
    Public Sub ListBoxItem_DoubleClick(sender As Object, e As MouseButtonEventArgs)
        RunWindow(CType(winList.SelectedItem, Type))
    End Sub



    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        RunWindow(CType(winList.SelectedItem, Type))
    End Sub
End Class

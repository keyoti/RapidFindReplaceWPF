Imports System.Reflection

Namespace ViewModels

    Public Class MainWindowViewModel
        Private windows As List(Of Type) = Nothing

        Public ReadOnly Property AvailableWindows As List(Of Type)
            Get
                If (windows Is Nothing) Then
                    Dim children = Assembly.GetExecutingAssembly().GetTypes().Where(Function(t) GetType(System.Windows.Window).IsAssignableFrom(t))
                    
                    windows = New List(Of Type)
                    For Each t As Type In children
                        If (t.Name <> "MainWindow") Then
                            windows.Add(t)
                        End If
                        If (t.Name = "Basic") Then
                            SelectedWindow = t
                        End If
                    Next
                End If

                Return windows
            End Get
        End Property

        Private _SelectedWindow As Type

        Public Property SelectedWindow As Type
            Get
                Return _SelectedWindow
            End Get
            Set(value As Type)
                _SelectedWindow = value
            End Set
        End Property

    End Class
End Namespace

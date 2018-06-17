VERSION 1.0 CLASS
BEGIN
  MultiUse = -1  'True
END
Attribute VB_Name = "Sheet1"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = True
' Copyright 2018 hrktir
'
' Redistribution and use in source and binary forms, with or without modification,
' are permitted provided that the following conditions are met:
'
' 1. Redistributions of source code must retain the above copyright notice,
'    this list of conditions and the following disclaimer.
'
' 2. Redistributions in binary form must reproduce the above copyright notice,
'    this list of conditions and the following disclaimer in the documentation and/or
'    other materials provided with the distribution.
'
' 3. Neither the name of the copyright holder nor the names of its contributors
'    may be used to endorse or promote products derived from this software without
'    specific prior written permission.
'
' THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
' ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
' WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
' IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT,
' INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
' BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
' DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
' LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE
' OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED
' OF THE POSSIBILITY OF SUCH DAMAGE.

Sub OutputCSV()
  
  Dim idx As Long
  Dim lists As String
  Dim shtname As String
  Dim overwriteOk As Boolean
  
  ' Don't display alert when to output
  overwriteOk = Not CheckBox1.Value
  
  Application.DisplayAlerts = overwriteOk
  
  For idx = 1 To Sheets.Count
    If Sheets(idx).Name <> "_TOP" Then
      shtname = Sheets(idx).Name & ".csv"
      
      lists = lists & shtname & vbCrLf
      
      Sheets(idx).Select
      Sheets(idx).Copy
      
      ActiveWorkbook.SaveAs Filename:=shtname, FileFormat:=xlCSV, CreateBackup:=False
      ActiveWindow.Close
      
    End If
    
  Next idx
  
  Sheets("_TOP").Select
  
  MsgBox (lists)
End Sub



Private Sub CommandButton1_Click()
  Call OutputCSV
End Sub

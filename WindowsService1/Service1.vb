Imports System.Xml
Imports System.Net
Imports System.IO
Imports System.Text

Public Class Service1
    Const siteFTP As String = "ftp://ftp.myftpsite.com/PositionMartin.txt"
    Const usager As String = "usager"
    Const motDePasse As String = "passwd"
    Const repertoire As String = "c:\temp"
    Const fichierTxt As String = repertoire + "\PositionMartin.txt"
    Const tempMiseAJour As Integer = 15
    Public thisTimer As System.Timers.Timer
    Protected Overrides Sub OnStart(ByVal args() As String)

        ' Create and start a timer that checks every 15 minutes.
        thisTimer = New System.Timers.Timer()
        thisTimer.Enabled = True
        thisTimer.Interval = tempMiseAJour * 60000 'Temps en milisecondes. 
        thisTimer.AutoReset = True
        AddHandler thisTimer.Elapsed, AddressOf thisTimer_tick
        thisTimer.Start()

        GeoIP()


    End Sub

    Protected Overrides Sub OnStop()
        ' Add code here to perform any tear-down necessary to stop your service.
        thisTimer.Stop()
        thisTimer.Enabled = False

    End Sub

    Private Sub thisTimer_tick()
        GeoIP()
    End Sub

    Private Sub GeoIP()
        Dim xmldoc As New XmlDocument

        Dim Latitude As String
        Dim Longitude As String

        Try

            xmldoc.Load("http://freegeoip.net/xml/")
            Latitude = xmldoc.GetElementsByTagName("Latitude").Item(0).InnerText
            Longitude = xmldoc.GetElementsByTagName("Longitude").Item(0).InnerText

            CreationFichierTexte(Longitude, Latitude)
        Catch e As Exception 'Si pas de connexion Internet
            Dim fichierLog As New StreamWriter(repertoire + "\logPositionAGOL.txt", True)
            fichierLog.WriteLine("ERREUR le " + DateTime.Now.ToString)
            fichierLog.WriteLine(e.Message.ToString)
            fichierLog.WriteLine("****Fin*****")
            fichierLog.Close()
        End Try


    End Sub

    Private Sub CreationFichierTexte(Longitude As String, Latitude As String)
        Dim Path As String = fichierTxt
        Dim heure As String = DateTime.Now.Hour.ToString + ":" + minuteAvec2Chiffre()

        Dim dateJour As String = Date.Now.Month.ToString + "/" + Date.Now.Day.ToString + "/" + Date.Now.Year.ToString
        If File.Exists(Path) Then
            File.Delete(Path)
        End If
        Dim fichier As New StreamWriter(fichierTxt)
        fichier.WriteLine("nom,x,y,heure,jour")
        fichier.WriteLine("Martin," + Longitude + "," + Latitude + "," + heure + "," + dateJour)
        fichier.Close()


        UploadFTP(Path)

    End Sub

    Private Function minuteAvec2Chiffre() As String
        Dim retourMinute As String
        Dim minute As Integer = DateTime.Now.Minute

        If minute < 10 Then
            retourMinute = "0" + minute.ToString
        Else
            retourMinute = minute.ToString
        End If

        Return retourMinute
    End Function
    Private Sub UploadFTP(path As String)
        Dim request As FtpWebRequest = DirectCast(FtpWebRequest.Create(siteFTP), FtpWebRequest)
        request.Method = WebRequestMethods.Ftp.UploadFile

        request.Credentials = New NetworkCredential(usager, motDePasse) '***** Mettre les bons
        'request.UseBinary = True



        Dim sourceStream As New StreamReader(path)

        Dim fileContents As Byte() = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd())

        sourceStream.Close()

        request.ContentLength = fileContents.Length

        'CType(clsRequest.GetResponse(), FtpWebResponse)
        Dim requestStream As System.IO.Stream = request.GetRequestStream
        requestStream.Write(fileContents, 0, fileContents.Length)
        requestStream.Close()


        Dim response As FtpWebResponse = request.GetResponse

        response.Close()

    End Sub


End Class

<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<%@ Page Language="c#" AutoEventWireup="false" Codebehind="Image_Font.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.Image_Font" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>Image_Font</title>
		<meta content="Microsoft Visual Studio .NET 7.1" name="GENERATOR">
		<meta content="Visual Basic .NET 7.1" name="CODE_LANGUAGE">
		<meta content="JavaScript" name="vs_defaultClientScript">
		<meta content="http://schemas.microsoft.com/intellisense/ie5" name="vs_targetSchema">
		<script LANGUAGE="JavaScript">
        function ScriptTest()
        {
            var theform;
		    if (window.navigator.appName.toLowerCase().indexOf("netscape") > -1) {
			    theform = document.forms["Form1"];
		    }
		    else {
			    theform = document.Form1;
		    }
        }
		</script>
	</HEAD>
	<body MS_POSITIONING="GridLayout">
		<form id="Form1" method="post" runat="server">
			<cc1:GHTSubTest id="GHTSubTest1" style="Z-INDEX: 101; LEFT: 16px; POSITION: absolute; TOP: 15px"
				runat="server" Width="40px" Height="40px">
				<asp:Image id="Image1" runat="server" Font-Bold="False" Font-Italic="False" Font-Name="David"
					Font-Overline="False" Font-Size="12" Font-Strikeout="False" Font-Underline="False"></asp:Image>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest6" style="Z-INDEX: 106; LEFT: 16px; POSITION: absolute; TOP: 72px"
				runat="server" Height="40px" Width="40px">
				<asp:Image id="Image6" runat="server" Font-Bold="True" Font-Italic="True" Font-Overline="True"
					Font-Size="12" Font-Strikeout="True" Font-Underline="True" Font-Names="David, Tahoma, Arial"></asp:Image>
			</cc1:GHTSubTest>
		</form>
		<br>
		<br>
	</body>
</HTML>

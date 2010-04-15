<?xml version='1.0' encoding='ISO-8859-1'?>
<xsl:stylesheet version='1.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
<xsl:template match='/'>
<html>
<body style='FONT-FAMILY: arial; FONT-SIZE: 12px'>
<style type='text/css'>
table.t1 { background-color: #FEFEF2; }tr.da { background-color: #E0ECF8; }
tr.db { background-color: #EFFBEF; }
tr.f { background-color: #F5A9A9; }
tr.dh th { background-color: #FCF6CF; }
tr.d0 td { background-color: #FCF6CF; }
tr.d1 td { background-color: #FEFEF2; }
</style>
<table width='100%'>
<tr class='da'><td width='20%'>Synchronization directory</td>
<td>: <xsl:value-of select='syncdirectory/@value'/></td></tr>
</table>
<br></br><br></br>
<xsl:for-each select='syncdirectory/syncsession'>
<table class='t1' width='100%'>
<tr><td width='20%'>Job Name</td><td>: <xsl:value-of select='profilename'/></td></tr>
<tr><td width='20%'>Date</td><td>: <xsl:value-of select='syncdate'/></td></tr>
<tr><td width='20%'>Storage directory</td><td>: <xsl:value-of select='storagedirectory'/></td></tr>
<tr><td width='20%'>Synchronization direction</td><td>: <xsl:value-of select='syncdirection'/></td></tr>
<tr><td width='20%'>Number of files processed</td><td>: <xsl:value-of select='numberoffilesprocessed'/></td></tr>
<xsl:if test='syncrecord'>
<tr><td width='20%'>Start time</td><td>: <xsl:value-of select='starttime'/></td></tr>
<tr><td width='20%'>End time</td><td>: <xsl:value-of select='endtime'/></td></tr>
<tr>
<td colspan='2'>
<table width='100%' cellpadding='3'>
<tr class='dh' align='left'><th width='75%'>File</th><th>Action</th><th>Status</th></tr>
<xsl:for-each select='syncrecord'>
<xsl:choose>
<xsl:when test="status='FAIL'">
<tr class='f'>
<td width='75%'><xsl:value-of select='file'/></td>
<td width='75%'><xsl:value-of select='action'/></td>
<td width='75%'><xsl:value-of select='status'/></td>
</tr>
</xsl:when>
<xsl:otherwise>
<xsl:choose>
<xsl:when test='(position() mod 2 = 1)'>
<tr class='d1'>
<td width='75%'><xsl:value-of select='file'/></td>
<td width='75%'><xsl:value-of select='action'/></td>
<td width='75%'><xsl:value-of select='status'/></td>
</tr>
</xsl:when>
<xsl:otherwise>
<tr class='d0'>
<td width='75%'><xsl:value-of select='file'/></td>
<td width='75%'><xsl:value-of select='action'/></td>
<td width='75%'><xsl:value-of select='status'/></td>
</tr>
</xsl:otherwise>
</xsl:choose>
</xsl:otherwise>
</xsl:choose>
</xsl:for-each>
</table>
</td>
</tr>
</xsl:if>
</table>
<br></br><br></br><br></br><br></br>
</xsl:for-each>
</body>
</html>
</xsl:template>
</xsl:stylesheet>

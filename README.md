# 基础功能实现
<table>
<capital>Standard2.0</capital>
<tr>
<th>类名</th>
<th>方法</th>
<th>参数</th>
<th>描述</th>
<tr>
<th rowspan="4">BitHelper</th>
<th rowspan="2">SetBit</th>
<th>(byte[] data, int bitIndex, bool value)</th>
<th>传入指定字节数组,Bit的指定位置和对应值</th>
<tr>
<th>(byte data, int bitIndex, bool value)</th>
<th>传入指定字节,Bit的指定位置和对应值</th>
<tr>
<th rowspan="2">(bool)GetBit</th>
<th>(byte[] data, int bitIndex)</th>
<th>传入指定字节数组,Bit的指定位置和对应值,返回bool值,1:true;0:false</th>
<tr>
<th>(byte data, int bitIndex)</th>
<th>传入指定字节,Bit的指定位置和对应值,返回bool值,1:true;0:false</th>
<tr>
<th>FileHelper</th>
<th>CheckFileOrDirectoryNameValidity</th>
<th>(string name)</th>
<th>检测文件或者文件夹名称是否合法</th>
</table>
<?xml version="1.0" encoding="utf-8"?>
<SchemeTable ID="6c977939-bbfc-456f-a133-f1c2244e3cc3" Partition="29f90c69-c1ef-4cbf-b9d5-7fc91cd68c67">
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="d35c4500-1566-49aa-ad00-9dc9364ccfab" Name="EDSCertificateInfo" Type="String(Max) Null">
		<Description>данные сертификата</Description>
	</SchemePhysicalColumn>
	<Predicate Dbms="SqlServer">[Login] IS NOT NULL AND [Login] &lt;&gt; N''</Predicate>
	<Predicate Dbms="PostgreSql">"Login" IS NOT NULL AND "Login" &lt;&gt; ''</Predicate>
</SchemeTable>
<?xml version="1.0" encoding="utf-8"?>
<SchemeTable IsSystem="true" IsPermanent="true" ID="57b9e507-d135-4c69-9a94-bf507d499484" Name="Configuration" Group="System">
	<Description>Configuration properties</Description>
	<SchemePhysicalColumn ID="47b91d0b-9866-4832-a04e-973b28b5c9f5" Name="BuildVersion" Type="String(24) Not Null">
		<Description>Platform build version</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="575beac6-5579-49ec-9157-ea6af610b969" Name="BuildName" Type="String(64) Not Null">
		<Description>Platform build version as text, for example "2.5 beta"</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="acaecb91-b71d-4923-9a68-aa6eb5fdeecf" Name="BuildDate" Type="Date Not Null">
		<Description>Platform build date</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="12519720-b4c0-4d23-bb9f-b6b244beab13" Name="Description" Type="String(512) Null">
		<Description>Configuration description</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="acefad4b-7c0a-40c3-8364-3fd6caa1026d" Name="Modified" Type="DateTime Not Null">
		<Description>Date/time of the last changes</Description>
	</SchemePhysicalColumn>
	<SchemeComplexColumn ID="77f607d3-20e5-4d75-9089-1d5b4988be56" Name="ModifiedBy" Type="Reference(Typified) Not Null" ReferencedTable="6c977939-bbfc-456f-a133-f1c2244e3cc3" WithForeignKey="false">
		<Description>Employee who made recent changes</Description>
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="77f607d3-20e5-0075-4000-0d5b4988be56" Name="ModifiedByID" Type="Guid Not Null" ReferencedColumn="6c977939-bbfc-016f-4000-01c2244e3cc3" />
		<SchemeReferencingColumn ID="fe93a86b-9c36-4b67-b721-8063dba3a8c0" Name="ModifiedByName" Type="String(128) Not Null" ReferencedColumn="1782f76a-4743-4aa4-920c-7edaee860964" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn ID="c9de3895-668c-4921-88c2-123c4424cdda" Name="Version" Type="Int32 Not Null">
		<Description>Configuration version</Description>
	</SchemePhysicalColumn>
</SchemeTable>
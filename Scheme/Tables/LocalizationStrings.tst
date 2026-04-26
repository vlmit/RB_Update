<?xml version="1.0" encoding="utf-8"?>
<SchemeTable IsSealed="true" ID="3e0ef7dd-7303-41e8-9ddc-af0a30e0de84" Name="LocalizationStrings" Group="System">
	<SchemeComplexColumn ID="3e0ef7dd-7303-00e8-2000-0f0a30e0de84" Name="Entry" Type="Reference(Typified) Not Null" ReferencedTable="b92e97c0-4557-4d43-874a-e9a75173cbf8" WithForeignKey="false">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="3e0ef7dd-7303-01e8-4000-0f0a30e0de84" Name="EntryID" Type="Guid Not Null" ReferencedColumn="a1fcd6b6-eba9-4619-9f95-34e84e7b931e" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn ID="b78e1c06-4f32-4f5c-8590-73626d28254e" Name="Culture" Type="Int32 Not Null" />
	<SchemePhysicalColumn ID="f6c45aeb-7bc5-405f-ae46-6761b1dbc073" Name="Value" Type="String(1024) Null" />
	<SchemeIndex ID="f8638e6e-6f23-4085-beec-0c84113269e8" Name="idx_LocalizationStrings_EntryIDCulture" IsClustered="true">
		<SchemeIndexedColumn Column="3e0ef7dd-7303-01e8-4000-0f0a30e0de84" />
		<SchemeIndexedColumn Column="b78e1c06-4f32-4f5c-8590-73626d28254e" />
	</SchemeIndex>
</SchemeTable>
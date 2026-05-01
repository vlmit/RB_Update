<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="e1b3cb70-f7fc-4b62-b822-e5302d673356" ID="67febfaf-f5f5-4b3d-9363-8a5053c38d79" Name="SampleText" Group="Junicsoft" InstanceType="Cards" ContentType="Entries">
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="67febfaf-f5f5-003d-2000-0a5053c38d79" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="1074eadd-21d7-4925-98c8-40d1e5f0ca0e">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="67febfaf-f5f5-013d-4000-0a5053c38d79" Name="ID" Type="Guid Not Null" ReferencedColumn="9a58123b-b2e9-4137-9c6c-5dab0ec02747" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn ID="3686b71d-5441-4cbc-a2c8-c19eebb66f3f" Name="Text" Type="String(Max) Not Null" />
	<SchemeComplexColumn ID="b43427c7-2549-4242-b8f5-7da6be40028a" Name="Author" Type="Reference(Typified) Null" ReferencedTable="6c977939-bbfc-456f-a133-f1c2244e3cc3">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="b43427c7-2549-0042-4000-0da6be40028a" Name="AuthorID" Type="Guid Null" ReferencedColumn="6c977939-bbfc-016f-4000-01c2244e3cc3" />
		<SchemeReferencingColumn ID="f5792bf5-f55f-4505-aaf3-1817bea21f36" Name="AuthorName" Type="String(128) Null" ReferencedColumn="1782f76a-4743-4aa4-920c-7edaee860964" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn ID="e98579a8-fbf7-4a45-adc6-c2afca788307" Name="Status" Type="Boolean Not Null">
		<SchemeDefaultConstraint IsPermanent="true" ID="8167672c-ce1d-4f38-a0cf-38b7aaeae8d4" Name="df_SampleText_Status" Value="false" />
	</SchemePhysicalColumn>
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="67febfaf-f5f5-003d-5000-0a5053c38d79" Name="pk_SampleText" IsClustered="true">
		<SchemeIndexedColumn Column="67febfaf-f5f5-013d-4000-0a5053c38d79" />
	</SchemePrimaryKey>
</SchemeTable>